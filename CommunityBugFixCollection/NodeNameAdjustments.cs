using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings.Characters;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable IDE0060 // Remove unused parameter

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
            => "Backspace (\\b)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bell))]
        private static string BellNamePostfix(string __result)
            => "Bell (\\a)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CarriageReturn))]
        private static string CarriageReturnNamePostfix(string __result)
            => "Carriage Return (\\r)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormFeed))]
        private static string FormFeedNamePostfix(string __result)
            => "Form Feed (\\f)";

        private static bool Prepare() => Enabled;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Double))]
        private static string Remap11_01_DoubleNamePostfix(string __result)
            => "Remap [-1; 1] to [0; 1]";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Float))]
        private static string Remap11_01_FloatNamePostfix(string __result)
            => "Remap [-1; 1] to [0; 1]";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Space))]
        private static string SpaceNamePostfix(string __result)
            => "Space ( )";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tab))]
        private static string TabNamePostfix(string __result)
            => "Tab (\\t)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VerticalTab))]
        private static string VerticalTabNamePostfix(string __result)
            => "Vertical Tab (\\v)";
    }
}

#pragma warning restore IDE0060 // Remove unused parameter