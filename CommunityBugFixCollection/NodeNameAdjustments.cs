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
    [HarmonyPatch(nameof(ProtoFluxNode.NodeName), MethodType.Getter)]
    [HarmonyPatchCategory(nameof(NodeNameAdjustments))]
    internal sealed class NodeNameAdjustments : ResoniteMonkey<NodeNameAdjustments>
    {
        public override bool CanBeDisabled => true;

        private static bool Prepare() => Enabled;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Float))]
        private static string Remap11_01_FloatNamePostfix()
            => "Remap [-1; 1] to [0; 1]";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Double))]
        private static string Remap11_01_DoubleNamePostfix()
            => "Remap [-1; 1] to [0; 1]";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Backspace))]
        private static string BackspaceNamePostfix()
            => "Backspace (\\b)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Bell))]
        private static string BellNamePostfix()
            => "Bell (\\a)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CarriageReturn))]
        private static string CarriageReturnNamePostfix()
            => "Carriage Return (\\r)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FormFeed))]
        private static string FormFeedNamePostfix()
            => "Form Feed (\\f)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings.Characters.NewLine))]
        private static string NewLineCharNamePostfix()
            => "New Line (\\n)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Strings.NewLine))]
        private static string NewLineStringNamePostfix()
            => "Environment New Line";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Space))]
        private static string SpaceNamePostfix()
            => "Space ( )";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tab))]
        private static string TabNamePostfix()
            => "Tab (\\t)";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VerticalTab))]
        private static string VerticalTabNamePostfix()
            => "Vertical Tab (\\v)";
    }
}
