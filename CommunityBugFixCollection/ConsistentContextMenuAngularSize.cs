using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ConsistentContextMenuAngularSize))]
    [HarmonyPatch(typeof(ContextMenu), nameof(ContextMenu.OnCommonUpdate))]
    internal sealed class ConsistentContextMenuAngularSize : ResoniteMonkey<ConsistentContextMenuAngularSize>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(ContextMenu __instance)
        {
            if (!Enabled || !__instance.IsVisible || !__instance.IsUnderLocalUser)
                return;

            // Vanilla value for the magic constant
            var magicConstant = 0.2f;

            if (!__instance.LocalUser.VR_Active)
            {
                // This whole equation is based on a FOV of 60° as the default
                // The formula for the angular diameter of a plane is: a = 2 * atan(size / (2 * distance))
                // Invert to: tan(0.5 * a) and multiply with magic 0.34641 to get the vanilla value of 0.2 for 60°
                // Calculation of magic multiplier: https://www.wolframalpha.com/input?i2d=true&i=0.2+%3D+x*+Tan%5BDivide%5B%CF%80%2C180%5D+*.5+*+60+rad%5D
                // Graph of resulting magic constant replacement: https://www.wolframalpha.com/input?i2d=true&i=0.34641+*+Tan%5BDivide%5B%CF%80%2C180%5D*.5*x+rad%5D+for+10+%3C%3D+x+%3C%3D+120

                var fov = __instance.World.GetFOV();
                magicConstant = 0.34641f * MathX.Tan(MathX.Deg2Rad * 0.5f * fov);
            }

            var scale = float3.One * (magicConstant / __instance.Canvas.Size.Value.y);

            // Only update when scale is different from current -
            // ideally this would use a local field on the ContextMenu to check for the FOV changing.
            // Can't do a static value here because there's multiple ContextMenus, even for the local user,
            // and using a ConditionalWeakTable would be more overhead than it's worth.
            if ((MathX.Abs(scale - __instance.Canvas.Slot.LocalScale) > (0.000001f * float3.One)).Any())
                __instance.Canvas.Slot.LocalScale = scale;
        }
    }
}