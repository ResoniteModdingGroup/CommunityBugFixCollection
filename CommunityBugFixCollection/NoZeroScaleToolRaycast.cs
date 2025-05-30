using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(Tool), nameof(Tool.GetHit))]
    [HarmonyPatchCategory(nameof(NoZeroScaleToolRaycast))]
    internal sealed class NoZeroScaleToolRaycast : ResoniteBugFixMonkey<NoZeroScaleToolRaycast>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyPrefix]
        private static bool GetHitPrefix(Tool __instance)
            => !Enabled || (__instance.TipForward.IsValid() && __instance.TipForward != float3.Zero);
    }
}