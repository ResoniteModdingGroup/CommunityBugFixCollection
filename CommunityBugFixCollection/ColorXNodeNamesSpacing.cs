using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ColorXNodeNamesSpacing))]
    [HarmonyPatch(typeof(StringHelper), nameof(StringHelper.BeautifyName))]
    internal sealed class ColorXNodeNamesSpacing : ResoniteMonkey<ColorXNodeNamesSpacing>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static string Postfix(string __result)
            => __result.Replace("Color X", "ColorX ");

        private static bool Prepare() => Enabled;
    }
}