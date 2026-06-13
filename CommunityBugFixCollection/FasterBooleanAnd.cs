using ProtoFlux.Runtimes.Execution.Nodes.Operators;
using HarmonyLib;

using ExecutionContext = ProtoFlux.Runtimes.Execution.ExecutionContext;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FasterBooleanAnd))]
    [HarmonyPatch(typeof(AND_Bool), nameof(AND_Bool.Compute))]
    internal sealed class FasterBooleanAnd : ResoniteBugFixMonkey<FasterBooleanAnd>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static bool Prefix(AND_Bool __instance, ExecutionContext context, out bool __result)
        {
            __result = false;
            var aSource = __instance.A.Source;

            if (aSource is null || context.CurrentRuntime.EvaluateValue<bool>(aSource, context))
            {
                var bSource = __instance.B.Source;
                __result = bSource is null || context.CurrentRuntime.EvaluateValue<bool>(bSource, context);
            }

            return false;
        }
    }
}