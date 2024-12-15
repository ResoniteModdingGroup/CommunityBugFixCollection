using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(nameof(ProtoFluxNode.NodeName))]
    [HarmonyPatchCategory(nameof(NodeNameAdjustments))]
    internal sealed class NodeNameAdjustments : ResoniteMonkey<NodeNameAdjustments>
    {
        public override bool CanBeDisabled => true;

        private static bool Prepare() => Enabled;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Float))]
        private static string Remap11_01_FloatNamePostfix(string __result)
            => "Remap [-1; 1] to [0; 1]";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Remap11_01_Double))]
        private static string Remap11_01_DoubleNamePostfix(string __result)
            => "Remap [-1; 1] to [0; 1]";
    }
}
