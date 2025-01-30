using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(IndependentlyScaleDirectCursor))]
    [HarmonyPatch(typeof(InteractionLaser), nameof(InteractionLaser.UpdateLaserVisual))]
    internal sealed class IndependentlyScaleDirectCursor : ResoniteMonkey<IndependentlyScaleDirectCursor>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(InteractionLaser __instance)
        {
            if (!Enabled || !__instance._directCursorActive.Target.Value)
                return;

            // This is the same calculation as is done for the actual point, but for the direct one
            var localPoint = __instance._directCursorOffset.Target.Value;
            var localPointInUserRootPos = __instance.Slot.LocalPointToSpace(in localPoint, __instance.LocalUserRoot.Slot);
            var userViewInUserRootPos = __instance.LocalUserRoot.Slot.GlobalPointToLocal(__instance.World.LocalUserViewPosition);

            var distanceInUserRoot = MathX.Distance(in localPointInUserRootPos, in userViewInUserRootPos);
            var localDistance = __instance.Slot.SpaceScaleToLocal(distanceInUserRoot, __instance.LocalUserRoot.Slot);

            var localScaleFactor = !__instance.InputInterface.VR_Active
                ? localDistance * (__instance.World.LocalUserDesktopFOV / 60f)
                : 1f + (0.5f * MathX.Max(0f, localDistance - 1f));

            __instance._directCursorRoot.Target.LocalScale = localScaleFactor * float3.One;

            // Have to adjust the direct line to the new scale as well
            var directLineTargetPos = __instance._directLineMesh.Target.Slot.SpacePointToLocal(__instance._pointSlotPos.Target.Value, __instance.Slot);
            __instance._directLineTarget.Target.Value = directLineTargetPos;
            __instance._directCursorVisualsVisible.Value = directLineTargetPos.Magnitude > 0.01f;
        }
    }
}