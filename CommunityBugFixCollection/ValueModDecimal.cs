using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ValueModDecimal))]
    [HarmonyPatch(typeof(Elements.Core.Coder<Decimal>), nameof(Elements.Core.Coder<Decimal>.Mod))]
    internal sealed class ValueModDecimal : ResoniteMonkey<ValueModDecimal>
    {
        public override IEnumerable<string> Authors => Contributors.__Choco__;

        public override bool CanBeDisabled => true;

        private static bool Prefix(Decimal a, Decimal b)
        {
            if (b == 0)
            {
                return false;
            }
            return true;
        }
    }
}