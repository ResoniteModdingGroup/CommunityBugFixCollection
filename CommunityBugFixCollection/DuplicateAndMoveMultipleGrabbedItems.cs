using Elements.Core;
using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
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
        // Literally just a copy paste of Slot.Duplicate but with a tiny snippet added to the middle
        // would probably do this using a transpiler but it would require some kind of additional function argument to whether it should create undo steps
        // or I could check stack traces to see if it's called from within my code but that's a little jank

        public static Slot UndoableChildrenDuplicate(this Slot toDuplicate, Slot? duplicateRoot = null, bool keepGlobalTransform = true, DuplicationSettings? settings = null)
        {
            if (toDuplicate.IsRootSlot)
                throw new Exception("Cannot duplicate root slot");

            duplicateRoot ??= toDuplicate.Parent ?? toDuplicate.World.RootSlot;

            if (duplicateRoot.IsChildOf(toDuplicate))
                throw new Exception("Target for the duplicate hierarchy cannot be within the hierarchy of the source");

            using var internalReferences = new InternalReferences();
            var syncRefs = Pool.BorrowHashSet<ISyncRef>();
            var slots = Pool.BorrowHashSet<Slot>();
            var postDuplication = Pool.BorrowList<Action>();

            void DuplicationHandler(IDuplicationHandler handler)
            {
                handler.OnBeforeDuplicate(toDuplicate, out var onDuplicated);

                if (onDuplicated is not null)
                    postDuplication.Add(onDuplicated);
            }

            toDuplicate.ForeachComponentInChildren<IDuplicationHandler>(DuplicationHandler,
                includeLocal: false, cacheItems: true);

            toDuplicate.GenerateHierarchy(slots);
            toDuplicate.CollectInternalReferences(toDuplicate, internalReferences, syncRefs, slots);
            var duplicated = toDuplicate.InternalDuplicate(duplicateRoot, internalReferences, syncRefs, settings!);

            if (keepGlobalTransform)
                duplicated.CopyTransform(toDuplicate);

            internalReferences.TransferReferences(false);

            // arti stuff begin
            foreach (var child in duplicated.Children)
                child.CreateSpawnUndoPoint();
            // arti stuff end

            var duplicatedComponents = Pool.BorrowList<Component>();
            duplicated.GetComponentsInChildren(duplicatedComponents);

            foreach (var component in duplicatedComponents)
                component.RunDuplicate();

            Pool.Return(ref duplicatedComponents);
            Pool.Return(ref syncRefs);

            foreach (var postDuplicationAction in postDuplication)
                postDuplicationAction();

            Pool.Return(ref postDuplication);

            return duplicated;
        }
    }

    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(DuplicateAndMoveMultipleGrabbedItems))]
    internal sealed class DuplicateAndMoveMultipleGrabbedItems : ResoniteMonkey<DuplicateAndMoveMultipleGrabbedItems>
    {
        public override IEnumerable<string> Authors => Contributors.Art0007i;
        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionHandler), "DuplicateGrabbed", [])]
        public static bool DuplicateGrabbedPrefix(InteractionHandler __instance)
        {
            if (!Enabled)
                return true;

            Slot? tempHolder = null;
            Slot? dupeHolder = null;

            __instance.World.BeginUndoBatch("Undo.DuplicateGrabbed".AsLocaleKey());

            try
            {
                tempHolder = __instance.Grabber.Slot.AddSlot("Holder");

                foreach (var grabbedObject in __instance.Grabber.GrabbedObjects)
                {
                    if (__instance.Grabber.GrabbableGetComponentInParents<IDuplicateBlock>(grabbedObject.Slot, excludeDisabled: true) == null)
                        continue;

                    grabbedObject.Slot.SetParent(tempHolder, false);
                }

                // by the time the duplication is done the children will have already escaped using their Grabbable.OnDuplicate function
                // so I just copied the entire duplication sequence and made it create undo steps at the correct time.. kinda jank but works
                /*dupeHolder = __instance.Grabber.HolderSlot.Duplicate();

                foreach (var child in dupeHolder.Children)
                {
                    child.CreateSpawnUndoPoint();
                }*/

                dupeHolder = __instance.Grabber.HolderSlot.UndoableChildrenDuplicate();

                dupeHolder.GetComponentsInChildren<IGrabbable>().ForEach(static grabbable =>
                {
                    if (grabbable.IsGrabbed)
                    {
                        grabbable.Release(grabbable.Grabber);
                    }
                });

                tempHolder.Destroy(__instance.Grabber.HolderSlot, false);
            }
            catch (Exception ex)
            {
                __instance.Debug.Error("Exception duplicating items!\n" + ex);
            }

            dupeHolder!.FilterWorldElement()?.Destroy(false);
            tempHolder!.FilterWorldElement()?.Destroy(false);

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