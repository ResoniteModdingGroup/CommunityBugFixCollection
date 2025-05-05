using Elements.Core;
using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(SceneInspector))]
    [HarmonyPatchCategory(nameof(OwnInspectors))]
    internal sealed class OwnInspectors : ResoniteMonkey<OwnInspectors>
    {
        private static readonly ConditionalWeakTable<SceneInspector, ChangeData> _changeDataByInspector = new();

        public override bool CanBeDisabled => true;

        private static ChangeData GetOrCreateChangeDataFor(SceneInspector inspector)
        {
            if (!_changeDataByInspector.TryGetValue(inspector, out var changeData))
            {
                changeData = new ChangeData(inspector);
                _changeDataByInspector.Add(inspector, changeData);
            }

            return changeData;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SceneInspector.OnAwake))]
        private static void OnAwakePostfix(SceneInspector __instance)
        {
            __instance.Root.OnTargetChange += syncRef => __instance.RunInUpdates(0, () =>
            {
                if (!syncRef.IsSyncDirty)
                    return;

                __instance._currentRoot.Target = syncRef.Target;
                GetOrCreateChangeDataFor(__instance).HasRootChange.Value = true;
            });

            __instance.ComponentView.OnTargetChange += syncRef => __instance.RunInUpdates(0, () =>
            {
                if (!syncRef.IsSyncDirty)
                    return;

                __instance._currentComponent.Target = syncRef.Target;
                GetOrCreateChangeDataFor(__instance).HasComponentChange.Value = true;
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.OnChanges))]
        private static bool OnChangesPrefix(SceneInspector __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.Root.IsTargetRemoved)
            {
                __instance.Slot.Destroy();
                return false;
            }

            var changeData = GetOrCreateChangeDataFor(__instance);

            if (changeData.HasRootChange || (__instance.World.IsAuthority && __instance.Root.Target != __instance._currentRoot.Target))
            {
                __instance._hierarchyContentRoot.Target.DestroyChildren();
                __instance._currentRoot.Target = __instance.Root.Target;
                __instance._rootText.Target.Value = "Root: " + (__instance._currentRoot.Target?.Name ?? "<i>null</i>");

                changeData.HasRootChange.Value = false;

                if (__instance._currentRoot.Target is not null)
                    __instance._hierarchyContentRoot.Target.AddSlot("HierarchyRoot").AttachComponent<SlotInspector>().Setup(__instance._currentRoot.Target, __instance._currentComponent);
            }

            if (changeData.HasComponentChange || (__instance.World.IsAuthority && __instance.ComponentView.Target != __instance._currentComponent.Target))
            {
                //_currentComponent.Target?.RemoveGizmo();

                if (__instance.ComponentView.Target is not null && !__instance.ComponentView.Target.IsRootSlot)
                    __instance.ComponentView.Target.GetGizmo();

                __instance._componentsContentRoot.Target.DestroyChildren();
                __instance._currentComponent.Target = __instance.ComponentView.Target!;
                __instance._componentText.Target.Value = "Slot: " + (__instance._currentComponent.Target?.Name ?? "<i>null</i>");

                changeData.HasComponentChange.Value = false;

                if (__instance._currentComponent.Target is not null)
                    __instance._componentsContentRoot.Target.AddSlot("ComponentRoot").AttachComponent<WorkerInspector>().SetupContainer(__instance._currentComponent.Target);
            }

            return false;
        }

        /*
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.OnInsertParentPressed))]
        private static bool OnInsertParentPressedPrefix(SceneInspector __instance)
        {
            // This patch is in conflict with Bounded UIX
            if (!Enabled)
                return true;

            if (__instance.ComponentView.Target is null)
                return false;

            var target = __instance.ComponentView.Target;
            __instance.World.BeginUndoBatch(__instance.GetLocalized("Undo.InsertParent", null!, "name", target.Name));

            var parent = target.Parent.AddSlot(target.Name + " - Parent");
            parent.CopyTransform(target);
            parent.CreateSpawnUndoPoint();

            target.CreateTransformUndoState(parent: true);
            target.SetParent(parent);
            target.SetIdentityTransform();

            // Set true and then force set undoable true so it will be undone to true
            // This allows the inspector to regenerate because changes is true
            var changeData = GetOrCreateChangeDataFor(__instance);
            changeData.HasRootChange.Value = true;
            changeData.HasRootChange.UndoableSet(true, true);

            __instance._currentRoot.UndoableSet(parent);
            __instance.Root.UndoableSet(parent);

            __instance.World.EndUndoBatch();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.OnObjectRootPressed))]
        private static bool OnObjectRootPressedPrefix(SceneInspector __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.Root.Target is null || __instance.Root.Target.IsRootSlot)
                return false;

            GetOrCreateChangeDataFor(__instance).HasRootChange.Value = true;

            var parentRoot = __instance.GetRoot(__instance.Root.Target.Parent);
            __instance._currentRoot.Target = parentRoot;
            __instance.Root.Target = parentRoot;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.OnRootUpPressed))]
        private static bool OnRootUpPressedPrefix(SceneInspector __instance)
        {
            if (!Enabled || __instance.Root.Target is null || __instance.Root.Target.IsRootSlot)
                return !Enabled;

            GetOrCreateChangeDataFor(__instance).HasRootChange.Value = true;

            var parent = __instance.Root.Target.Parent;
            __instance._currentRoot.Target = parent;
            __instance.Root.Target = parent;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.OnSetRootPressed))]
        private static bool OnSetRootPressedPrefix(SceneInspector __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.ComponentView.Target is null)
                return false;

            GetOrCreateChangeDataFor(__instance).HasRootChange.Value = true;
            __instance._currentRoot.Target = __instance.ComponentView.Target;
            __instance.Root.Target = __instance.ComponentView.Target;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneInspector.RunDestroy))]
        private static bool RunDestroyPrefix(SceneInspector __instance, Action<Slot> destroyAction)
        {
            if (!Enabled)
                return true;

            var target = __instance.ComponentView.Target;
            if (target is null || target.IsRootSlot)
                return false;

            Slot newTarget;
            var childIndex = target.ChildIndex;
            var parent = target.Parent;
            var isInspectorRoot = target == __instance.Root.Target;

            destroyAction(target);

            if (parent.IsRemoved || isInspectorRoot)
                newTarget = null!;
            else if (parent.ChildrenCount > 0)
                newTarget = parent[MathX.Min(childIndex, parent.ChildrenCount - 1)];
            else if (!parent.IsRootSlot)
                newTarget = parent;
            else
                newTarget = null!;

            GetOrCreateChangeDataFor(__instance).HasComponentChange.Value = true;

            __instance._currentComponent.Target = newTarget;
            __instance.ComponentView.Target = newTarget;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SlotRecord), nameof(SlotRecord.Pressed))]
        private static bool SlotRecordPressedPrefix(SlotRecord __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.Time.WorldTime - __instance._lastPress < .35
                && __instance.Slot.GetComponentInParents<SceneInspector>() is SceneInspector inspector)
            {
                GetOrCreateChangeDataFor(inspector).HasComponentChange.Value = true;
                inspector._currentComponent.Target = __instance.TargetSlot.Target;
                inspector.ComponentView.Target = __instance.TargetSlot.Target;

                foreach (var devTool in __instance.LocalUser.GetActiveTools().OfType<DevTool>())
                    devTool.SetActiveSlotGizmo(__instance.TargetSlot.Target);
            }

            __instance._lastPress = __instance.Time.WorldTime;
            return false;
        }*/

        private sealed class ChangeData
        {
            private readonly SceneInspector _inspector;

            public SyncField<bool> HasComponentChange { get; private set; }
            public SyncField<bool> HasRootChange { get; private set; }

            public ChangeData(SceneInspector inspector)
            {
                _inspector = inspector;

                HasComponentChange = GetField();
                HasRootChange = GetField();
            }

            private SyncField<bool> GetField()
            {
                var field = _inspector.Slot.AttachComponent<ValueField<bool>>();
                field.Persistent = false;

                return field.Value;
            }
        }
    }
}