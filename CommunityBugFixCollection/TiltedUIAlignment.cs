using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(TiltedUIAlignment))]
    [HarmonyPatch(typeof(UI_TargettingController), nameof(UI_TargettingController.OnBeforeHeadUpdate))]
    internal sealed class TiltedUIAlignment : ResoniteMonkey<TiltedUIAlignment>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(UI_TargettingController __instance)
        {
            if (!Enabled)
                return;

            var space = __instance.ViewSpace ?? __instance.Slot;
            var spacePosition = space.GlobalPosition;
            var rootDistance = MathX.Max(1, MathX.MaxComponent(MathX.Abs(spacePosition)));

            var rotationAxis = space.Right;
            var angle = MathX.Clamp(0.1f, 5, 0.01f * MathX.Sqrt(rootDistance));

            // Add angle to camera to prevent flickering
            var rotation = floatQ.AxisAngle(rotationAxis, angle);
            __instance.ViewRotation *= rotation;

            //  Adjust position based on angle to frame UI properly still
            var antiRotation = floatQ.AxisAngle(rotationAxis, -angle - 2f);
            __instance.ViewPosition = __instance._currentCenter + (space.GlobalRotationToLocal(antiRotation) * __instance._currentPlane * __instance._currentDistance);
        }
    }
}