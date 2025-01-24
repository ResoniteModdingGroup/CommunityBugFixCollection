using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(Tool), nameof(Tool.GetHit))]
    [HarmonyPatchCategory(nameof(NoZeroScaleToolRaycast))]
    internal sealed class NoZeroScaleToolRaycast : ResoniteMonkey<NoZeroScaleToolRaycast>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        private static bool GetHitPrefix(Tool __instance)
            => !Enabled || (__instance.TipForward.IsValid() && __instance.TipForward != float3.Zero);
    }
}