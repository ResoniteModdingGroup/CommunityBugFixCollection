using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(PMEUpdateOrder))]
    [HarmonyPatch(typeof(MemberEditor), nameof(MemberEditor.Setup))]
    internal sealed class PMEUpdateOrder : ResoniteMonkey<PMEUpdateOrder>
    {
        public override IEnumerable<string> Authors => Contributors.yosh;

        public override bool CanBeDisabled => true;

        private static void Postfix(MemberEditor __instance)
        {
            if (Enabled) {
                __instance.UpdateOrder = int.MaxValue;
            }
        }
    }
}