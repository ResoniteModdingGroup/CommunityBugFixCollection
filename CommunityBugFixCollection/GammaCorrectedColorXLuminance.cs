﻿using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(GammaCorrectedColorXLuminance))]
    [HarmonyPatch(typeof(colorX), nameof(colorX.Luminance), MethodType.Getter)]
    internal sealed class GammaCorrectedColorXLuminance : ResoniteMonkey<GammaCorrectedColorXLuminance>
    {
        public override bool CanBeDisabled => true;

        private static bool Prefix(colorX __instance, out float __result)
        {
            __result = __instance.ConvertProfile(ColorProfile.Linear).BaseColor.Luminance;

            return false;
        }

        private static bool Prepare() => Enabled;
    }
}