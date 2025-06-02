using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(NodeNameAdjustments))]
    [HarmonyPatch(nameof(ProtoFluxNode.NodeName), MethodType.Getter)]
    internal sealed class NodeNameAdjustments : ResoniteBugFixMonkey<NodeNameAdjustments>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

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
    }
}