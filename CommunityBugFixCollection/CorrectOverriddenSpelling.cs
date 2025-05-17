using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(CorrectOverriddenSpelling))]
    [HarmonyPatch(typeof(StringHelper), nameof(StringHelper.BeautifyName))]
    internal sealed class CorrectOverriddenSpelling : ResoniteMonkey<CorrectOverriddenSpelling>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static string Postfix(string __result)
        {
            if (!Enabled)
                return __result;

            return __result.Replace("Overriden", "Overridden");
        }
    }
}