using Elements.Core;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ValueModDecimal))]
    [HarmonyPatch(typeof(Coder<decimal>), nameof(Coder<Decimal>.Mod))]
    internal sealed class ValueModDecimal : ResoniteBugFixMonkey<ValueModDecimal>
    {
        public override IEnumerable<string> Authors => Contributors.__Choco__;

        private static bool Prefix(decimal b) => !Enabled || b != 0;
    }
}