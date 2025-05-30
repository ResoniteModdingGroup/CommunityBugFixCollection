using Elements.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(CorrectOverriddenSpelling))]
    [HarmonyPatch(typeof(StringHelper), nameof(StringHelper.BeautifyName))]
    internal sealed class CorrectOverriddenSpelling : ResoniteBugFixMonkey<CorrectOverriddenSpelling>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static string Postfix(string __result)
        {
            if (!Enabled)
                return __result;

            return __result.Replace("Overriden", "Overridden");
        }
    }
}