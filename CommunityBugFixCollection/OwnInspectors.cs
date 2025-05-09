using FrooxEngine;
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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SceneInspector.OnAwake))]
        private static void OnAwakePostfix(SceneInspector __instance)
        {
            __instance.Root.OnTargetChange += syncRef =>
            {
                if (!Enabled || !syncRef.IsSyncDirty || !__instance.World.CanMakeSynchronousChanges)
                    return;

                __instance._currentRoot.Target = syncRef.Target;
                _changeDataByInspector.GetOrCreateValue(__instance).HasRootChange = true;
            };

            __instance.ComponentView.OnTargetChange += syncRef =>
            {
                if (!Enabled || !syncRef.IsSyncDirty || !__instance.World.CanMakeSynchronousChanges)
                    return;

                var changeData = _changeDataByInspector.GetOrCreateValue(__instance);
                changeData.PreviousComponent = __instance._currentComponent;
                changeData.HasComponentChange = true;

                __instance._currentComponent.Target = syncRef.Target;
            };
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

            var changeData = _changeDataByInspector.GetOrCreateValue(__instance);

            if (changeData.HasRootChange || (__instance.World.IsAuthority && __instance.Root.Target != __instance._currentRoot.Target))
            {
                __instance._hierarchyContentRoot.Target.DestroyChildren();
                __instance._currentRoot.Target = __instance.Root.Target;
                __instance._rootText.Target.Value = "Root: " + (__instance._currentRoot.Target?.Name ?? "<i>null</i>");

                changeData.HasRootChange = false;

                if (__instance._currentRoot.Target is not null)
                    __instance._hierarchyContentRoot.Target.AddSlot("HierarchyRoot").AttachComponent<SlotInspector>().Setup(__instance._currentRoot.Target, __instance._currentComponent);
            }

            if (changeData.HasComponentChange || (__instance.World.IsAuthority && __instance.ComponentView.Target != __instance._currentComponent.Target))
            {
                (changeData.PreviousComponent ?? __instance._currentComponent.Target)?.RemoveGizmo();

                if (__instance.ComponentView.Target is not null && !__instance.ComponentView.Target.IsRootSlot)
                    __instance.ComponentView.Target.GetGizmo();

                __instance._componentsContentRoot.Target.DestroyChildren();
                __instance._currentComponent.Target = __instance.ComponentView.Target!;
                __instance._componentText.Target.Value = "Slot: " + (__instance._currentComponent.Target?.Name ?? "<i>null</i>");

                changeData.HasComponentChange = false;
                changeData.PreviousComponent = null;

                if (__instance._currentComponent.Target is not null)
                    __instance._componentsContentRoot.Target.AddSlot("ComponentRoot").AttachComponent<WorkerInspector>().SetupContainer(__instance._currentComponent.Target);
            }

            return false;
        }

        private sealed class ChangeData
        {
            public bool HasComponentChange { get; set; }

            public bool HasRootChange { get; set; }

            public Slot? PreviousComponent { get; set; }
        }
    }
}