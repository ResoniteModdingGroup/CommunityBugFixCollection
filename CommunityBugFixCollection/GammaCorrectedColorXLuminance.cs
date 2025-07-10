using Elements.Core;
using HarmonyLib;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(GammaCorrectedColorXLuminance))]
    [HarmonyPatch(typeof(colorX), nameof(colorX.Luminance), MethodType.Getter)]
    internal sealed class GammaCorrectedColorXLuminance : ResoniteBugFixMonkey<GammaCorrectedColorXLuminance>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static bool Prefix(colorX __instance, ref float __result)
        {
            if (!Enabled)
                return true;

            __result = __instance.ConvertProfile(ColorProfile.Linear).BaseColor.Luminance;

            return false;
        }
    }
}