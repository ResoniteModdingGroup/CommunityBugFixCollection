using Elements.Core;
using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    public static class DuplicateExtensions
    {
        // Literally just a copy paste of Slot.Duplicate but with a tiny snippet added to the middle
        // would probably do this using a transpiler but it would require some kind of additional function argument to whether it should create undo steps
        // or I could check stack traces to see if it's called from within my code but that's a little jank

        public static Slot UndoableChildrenDuplicate(this Slot toDuplicate, Slot duplicateRoot = null, bool keepGlobalTransform = true, DuplicationSettings settings = null)
        {
            if (toDuplicate.IsRootSlot)
            {
                throw new Exception("Cannot duplicate root slot");
            }
            if (duplicateRoot == null)
            {
                duplicateRoot = toDuplicate.Parent ?? toDuplicate.World.RootSlot;
            }
            else if (duplicateRoot.IsChildOf(toDuplicate))
            {
                throw new Exception("Target for the duplicate hierarchy cannot be within the hierarchy of the source");
            }
            //InternalReferences internalReferences = new InternalReferences();
            var internalReferences = typeof(Worker).GetNestedType("InternalReferences", AccessTools.all).GetConstructor(new Type[] { }).Invoke(null);
            HashSet<ISyncRef> hashSet = Pool.BorrowHashSet<ISyncRef>();
            HashSet<Slot> hashSet2 = Pool.BorrowHashSet<Slot>();
            List<Action> postDuplication = Pool.BorrowList<Action>();
            toDuplicate.ForeachComponentInChildren(delegate (IDuplicationHandler h)
            {
                h.OnBeforeDuplicate(toDuplicate, out var onDuplicated);
                if (onDuplicated != null)
                {
                    postDuplication.Add(onDuplicated);
                }
            }, includeLocal: false, cacheItems: true);
            toDuplicate.GenerateHierarchy(hashSet2);
            DuplicateFix.Debug("traverse1");
            Traverse.Create(toDuplicate).Method("CollectInternalReferences", toDuplicate, internalReferences, hashSet, hashSet2).GetValue();
            DuplicateFix.Debug("traverse2");
            Slot slot = (Slot)typeof(Slot).GetMethod("InternalDuplicate", AccessTools.all).Invoke(toDuplicate, new object[] { duplicateRoot, internalReferences, hashSet, settings });
            if (keepGlobalTransform)
            {
                slot.CopyTransform(toDuplicate);
            }
            DuplicateFix.Debug("traverse3");
            Traverse.Create(internalReferences).Method("TransferReferences", false).GetValue();
            List<Component> list = Pool.BorrowList<Component>();
            slot.GetComponentsInChildren(list);
            // arti stuff begin
            foreach (var child in slot.Children)
            {
                child.CreateSpawnUndoPoint();
            }
            // arti stuff end
            var runDuplicateMethod = typeof(Component).GetMethod("RunDuplicate", AccessTools.all);
            foreach (Component item in list)
            {
                runDuplicateMethod.Invoke(item, null);
            }
            Pool.Return(ref list);
            Pool.Return(ref hashSet);

            DuplicateFix.Debug("traverse4");
            Traverse.Create(internalReferences).Method("Dispose").GetValue();
            foreach (Action item2 in postDuplication)
            {
                item2();
            }
            Pool.Return(ref postDuplication);
            return slot;
        }
    }

    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(DuplicateAndMoveMultipleGrabbedItems))]
    internal sealed class DuplicateAndMoveMultipleGrabbedItems : ResoniteMonkey<DuplicateAndMoveMultipleGrabbedItems>
    {
        public override IEnumerable<string> Authors => Contributors.Art0007i;
        public override bool CanBeDisabled => true;

        [HarmonyPatch(typeof(InteractionHandler), "DuplicateGrabbed", new Type[] { })]
        private class DuplicateFixPatch
        {
            public static bool Prefix(InteractionHandler __instance)
            {
                if (!config.GetValue(KEY_ENABLED)) return true;

                Slot tempHolder = null;
                Slot dupeHolder = null;
                __instance.World.BeginUndoBatch("Undo.DuplicateGrabbed".AsLocaleKey());
                try
                {
                    tempHolder = __instance.Grabber.Slot.AddSlot("Holder");
                    foreach (IGrabbable grabbedObject in __instance.Grabber.GrabbedObjects)
                    {
                        if (__instance.Grabber.GrabbableGetComponentInParents<IDuplicateBlock>(grabbedObject.Slot, null, excludeDisabled: true) != null)
                        {
                            grabbedObject.Slot.SetParent(tempHolder, false);
                            continue;
                        }
                    }

                    // by the time the duplication is done the children will have already escaped using their Grabbable.OnDuplicate function
                    // so I just copied the entire duplication sequence and made it create undo steps at the correct time.. kinda jank but works
                    /*dupeHolder = __instance.Grabber.HolderSlot.Duplicate();

                    foreach (var child in dupeHolder.Children)
                    {
                        child.CreateSpawnUndoPoint();
                    }*/
                    dupeHolder = __instance.Grabber.HolderSlot.UndoableChildrenDuplicate();

                    dupeHolder.GetComponentsInChildren<IGrabbable>().ForEach(delegate (IGrabbable g)
                    {
                        if (g.IsGrabbed)
                        {
                            g.Release(g.Grabber);
                        }
                    });
                    tempHolder.Destroy(__instance.Grabber.HolderSlot, false);
                }
                catch (Exception ex)
                {
                    __instance.Debug.Error("Exception duplicating items!\n" + ex);
                }
                if (dupeHolder != null && !dupeHolder.IsRemoved) dupeHolder.Destroy(false);
                if (tempHolder != null && !tempHolder.IsRemoved) tempHolder.Destroy(false);

                __instance.World.EndUndoBatch();

                return false;
            }
        }

        [HarmonyPatch(typeof(Userspace), nameof(Userspace.Paste))]
        private class UserspacePastePatch
        {
            public static bool Prefix(Userspace __instance, Job<Slot> __result, SavedGraph data, Slot source, float3 userspacePos, floatQ userspaceRot, float3 userspaceScale, World targetWorld)
            {
                if (!config.GetValue(KEY_ENABLED)) return true;

                Job<Slot> task = new Job<Slot>();
                World world = targetWorld ?? Engine.Current.WorldManager.FocusedWorld;
                world.RunSynchronously(delegate
                {
                    Slot slot = null;
                    if (world.CanSpawnObjects())
                    {
                        float3 globalPosition = WorldManager.TransferPoint(userspacePos, Userspace.Current.World, world);
                        floatQ globalRotation = WorldManager.TransferRotation(userspaceRot, Userspace.Current.World, world);
                        float3 globalScale = WorldManager.TransferScale(userspaceScale, Userspace.Current.World, world);
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
                            slot.LoadObject(data.Root);
                            slot.GlobalPosition = globalPosition;
                            slot.GlobalRotation = globalRotation;
                            slot.GlobalScale = globalScale;
                            Traverse.Create(slot).Method("RunOnPaste").GetValue();
                            if (slot.Name == "Holder")
                            {
                                slot.Destroy(slot.Parent, false);
                            }
                        }
                    }
                    if (source != null && !source.IsDestroyed)
                    {
                        source.World.RunSynchronously(source.Destroy);
                    }
                    task.SetResultAndFinish(slot);
                });
                __result = task;
                return false;
            }
        }

        [HarmonyPatch(typeof(Grabber), nameof(Grabber.OnFocusChanged))]
        private class UserspaceTransferPatch
        {
            public static bool Prefix(Grabber __instance, World.WorldFocus focus)
            {
                if (!config.GetValue(KEY_ENABLED)) return true;

                if (!__instance.World.CanTransferObjectsOut())
                {
                    return false;
                }
                __instance.BeforeUserspaceTransfer?.Invoke();
                Traverse.Create(__instance).Method("CleanupGrabbed").GetValue();
                if (focus != 0 || !__instance.IsHoldingObjects)
                {
                    return false;
                }
                foreach (IGrabbable grabbedObject in __instance.GrabbedObjects)
                {
                    if (grabbedObject.Slot.GetComponentInChildren((IItemPermissions p) => !p.CanSave) != null)
                    {
                        return false;
                    }
                }
                float3 a = __instance.HolderSlot.LocalPosition;
                float3 b = float3.Zero;
                if (a != b)
                {
                    float3 b2 = __instance.HolderSlot.LocalPosition;
                    __instance.HolderSlot.LocalPosition = float3.Zero;
                    foreach (Slot child in __instance.HolderSlot.Children)
                    {
                        a = child.LocalPosition;
                        child.LocalPosition = a + b2;
                    }
                }

                if (Userspace.TryTransferToUserspaceGrabber(__instance.HolderSlot, __instance.LinkingKey))
                {
                    __instance.DestroyGrabbed();
                }
                return false;
            }
        }
    }
}