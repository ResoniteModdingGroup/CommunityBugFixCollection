using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings.Characters;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(NodeNameAdjustments))]
    [HarmonyPatch(nameof(ProtoFluxNode.NodeName), MethodType.Getter)]
    internal sealed class NodeNameAdjustments : ResoniteMonkey<NodeNameAdjustments>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

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

        private static bool Prepare() => Enabled;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Double))]
        private static string Remap11_01_DoubleNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Remap [-1; 1] to [0; 1]";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Float))]
        private static string Remap11_01_FloatNamePostfix(string __result)
        {
            if (!Enabled)
                return __result;

            return "Remap [-1; 1] to [0; 1]";
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