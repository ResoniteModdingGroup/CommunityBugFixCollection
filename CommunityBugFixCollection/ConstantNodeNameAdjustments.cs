using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings.Characters;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ConstantNodeNameAdjustments))]
    [HarmonyPatch(nameof(ProtoFluxNode.NodeName), MethodType.Getter)]
    internal sealed class ConstantNodeNameAdjustments : ResoniteBugFixMonkey<ConstantNodeNameAdjustments>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        // Legacy Fix, so off by default
        protected override bool OnComputeDefaultEnabledState() => false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Backspace))]
        private static string BackspaceNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Backspace (\\b)";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bell))]
        private static string BellNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Bell (\\a)";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CarriageReturn))]
        private static string CarriageReturnNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Carriage Return (\\r)";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormFeed))]
        private static string FormFeedNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Form Feed (\\f)";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Space))]
        private static string SpaceNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Space ( )";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tab))]
        private static string TabNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Tab (\\t)";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VerticalTab))]
        private static string VerticalTabNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Vertical Tab (\\v)";
        }
    }
}