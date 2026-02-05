using Elements.Core;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ValidQuaternionInputs))]
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.SpawnNode), [typeof(Type), typeof(Action<ProtoFluxNode>)])]
    internal sealed class ValidQuaternionInputs : ResoniteBugFixMonkey<ValidQuaternionInputs>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static void Postfix(ProtoFluxNode __result)
        {
            if (!Enabled)
                return;

            __result.RunInUpdates(0, () =>
            {
                if (__result is ValueInput<floatQ> floatQInput)
                {
                    floatQInput.Value.Value = floatQ.Identity;
                    return;
                }

                if (__result is ValueInput<doubleQ> doubleQInput)
                    doubleQInput.Value.Value = doubleQ.Identity;
            });
        }
    }
}