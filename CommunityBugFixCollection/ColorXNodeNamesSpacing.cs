using Elements.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ColorXNodeNamesSpacing))]
    [HarmonyPatch(typeof(StringHelper), nameof(StringHelper.BeautifyName))]
    internal sealed class ColorXNodeNamesSpacing : ResoniteBugFixMonkey<ColorXNodeNamesSpacing>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static string Postfix(string __result)
        {
            if (!Enabled)
                return __result;

            return __result.Replace("Color X", "ColorX ");
        }
    }
}