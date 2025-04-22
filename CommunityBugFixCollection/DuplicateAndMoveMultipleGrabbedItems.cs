using Elements.Core;
using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static FrooxEngine.Worker;

// This code is mainly decompiled Resonite source code,
// but the fix was devised by art0007i.
// Originally released under MIT-0 here:
// https://github.com/art0007i/DuplicateFix

namespace CommunityBugFixCollection
{
    internal static class DuplicateExtensions
    {
        // Literally just a copy paste of Slot.Duplicate but it duplicates several slots at same time
        public static void MultiDuplicate(this IEnumerable<Slot> toDuplicate, List<Slot> newSlots, Slot? duplicateRoot = null, bool keepGlobalTransform = true, DuplicationSettings? settings = null)
        {
            if (toDuplicate.Any(slot => slot.IsRootSlot))
                throw new Exception("Cannot duplicate root slot");

            if (duplicateRoot != null && toDuplicate.Any(slot => duplicateRoot.IsChildOf(slot)))
                throw new Exception("Target for the duplicate hierarchy cannot be within the hierarchy of the source");

            using var internalReferences = new InternalReferences();
            var breakRefs = Pool.BorrowHashSet<ISyncRef>();
            var hierarchy = Pool.BorrowHashSet<Slot>();
            var postDuplications = Pool.BorrowList<Action>();

            toDuplicate.Do(slot => slot.ForeachComponentInChildren<IDuplicationHandler>(handler =>
            {
                handler.OnBeforeDuplicate(slot, out var onDuplicated);

                if (onDuplicated != null)
                    postDuplications.Add(onDuplicated);
            }, includeLocal: false, cacheItems: true));

            toDuplicate.Do(slot => slot.GenerateHierarchy(hierarchy));
            toDuplicate.Do(slot => slot.CollectInternalReferences(slot, internalReferences, breakRefs, hierarchy));

            foreach (var slot in toDuplicate)
                newSlots.Add(slot.InternalDuplicate(duplicateRoot ?? slot.Parent ?? slot.World.RootSlot, internalReferences, breakRefs, settings!));

            if (keepGlobalTransform)
            {
                var i = 0;

                foreach (var slot in newSlots)
                    newSlots[i++].CopyTransform(slot);
            }

            internalReferences.TransferReferences(false);

            var newComponents = Pool.BorrowList<Component>();
            newSlots.Do(slot => slot.GetComponentsInChildren(newComponents));

            foreach (var item in newComponents)
                item.RunDuplicate();

            Pool.Return(ref newComponents);
            Pool.Return(ref breakRefs);

            foreach (var postDuplication in postDuplications)
                postDuplication();

            Pool.Return(ref postDuplications);
        }
    }

    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(DuplicateAndMoveMultipleGrabbedItems))]
    internal sealed class DuplicateAndMoveMultipleGrabbedItems : ResoniteMonkey<DuplicateAndMoveMultipleGrabbedItems>
    {
        public override IEnumerable<string> Authors => Contributors.Art0007i;
        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionHandler), nameof(InteractionHandler.DuplicateGrabbed), [])]
        public static bool DuplicateGrabbedPrefix(InteractionHandler __instance)
        {
            if (!Enabled)
                return true;

            var toDuplicate = Pool.BorrowList<Slot>();
            var newSlots = Pool.BorrowList<Slot>();

            __instance.World.BeginUndoBatch("Undo.DuplicateGrabbed".AsLocaleKey());

            try
            {
                foreach (var grabbedObject in __instance.Grabber.GrabbedObjects)
                {
                    if (__instance.Grabber.GrabbableGetComponentInParents<IDuplicateBlock>(grabbedObject.Slot, excludeDisabled: true) is not null)
                        continue;

                    toDuplicate.Add(grabbedObject.Slot.GetObjectRoot(__instance.Grabber.Slot));
                }

                toDuplicate.MultiDuplicate(newSlots);

                newSlots.Do(static slot => slot.CreateSpawnUndoPoint());

                newSlots.SelectMany(x => x.GetComponentsInChildren<IGrabbable>())
                    .Do(static x =>
                    {
                        if (x.IsGrabbed)
                            x.Release(x.Grabber);
                    });
            }
            catch (Exception ex)
            {
                __instance.Debug.Error("Exception duplicating items!\n" + ex);
            }

            Pool.Return(ref newSlots);
            Pool.Return(ref toDuplicate);

            __instance.World.EndUndoBatch();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Grabber), nameof(Grabber.OnFocusChanged))]
        public static bool OnFocusChangedPrefix(Grabber __instance, World.WorldFocus focus)
        {
            if (!Enabled)
                return true;

            if (!__instance.World.CanTransferObjectsOut())
                return false;

            __instance.BeforeUserspaceTransfer?.Invoke();
            __instance.CleanupGrabbed();

            if (focus != 0 || !__instance.IsHoldingObjects)
                return false;

            foreach (var grabbedObject in __instance.GrabbedObjects)
            {
                if (grabbedObject.Slot.GetComponentInChildren(static (IItemPermissions p) => !p.CanSave) is not null)
                    return false;
            }

            if (__instance.HolderSlot.LocalPosition != float3.Zero)
            {
                var holderOffset = __instance.HolderSlot.LocalPosition;
                __instance.HolderSlot.LocalPosition = float3.Zero;

                foreach (var child in __instance.HolderSlot.Children)
                    child.LocalPosition += holderOffset;
            }

            if (Userspace.TryTransferToUserspaceGrabber(__instance.HolderSlot, __instance.LinkingKey))
                __instance.DestroyGrabbed();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Userspace), nameof(Userspace.Paste))]
        public static bool PastePrefix(ref Job<Slot> __result, SavedGraph data, Slot source, float3 userspacePos, floatQ userspaceRot, float3 userspaceScale, World targetWorld)
        {
            if (!Enabled)
                return true;

            var task = new Job<Slot>();
            var world = targetWorld ?? Engine.Current.WorldManager.FocusedWorld;

            world.RunSynchronously(() =>
            {
                Slot? slot = null;

                if (world.CanSpawnObjects())
                {
                    var globalPosition = WorldManager.TransferPoint(userspacePos, Userspace.Current.World, world);
                    var globalRotation = WorldManager.TransferRotation(userspaceRot, Userspace.Current.World, world);
                    var globalScale = WorldManager.TransferScale(userspaceScale, Userspace.Current.World, world);

                    if (source?.World == world && !source.IsDestroyed)
                    {
                        source.ActiveSelf = true;
                        source.GlobalPosition = globalPosition;
                        source.GlobalRotation = globalRotation;
                        source.GlobalScale = globalScale;
                        task.SetResultAndFinish(source);
                    }
                    else
                    {
                        slot = world.AddSlot("Paste");
                        slot.LoadObject(data.Root, null!);
                        slot.GlobalPosition = globalPosition;
                        slot.GlobalRotation = globalRotation;
                        slot.GlobalScale = globalScale;

                        slot.RunOnPaste();

                        if (slot.Name == "Holder")
                            slot.Destroy(slot.Parent, false);
                    }
                }

                if (source!.FilterWorldElement() is not null)
                    source!.World.RunSynchronously(source.Destroy);

                task.SetResultAndFinish(slot!);
            });

            __result = task;
            return false;
        }
    }
}