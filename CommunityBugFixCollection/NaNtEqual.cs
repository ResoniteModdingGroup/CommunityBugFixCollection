using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using System;
using System.Collections.Generic;
using System.Text;
using ExecutionContext = ProtoFlux.Runtimes.Execution.ExecutionContext;

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(NaNtEqual))]
    internal sealed class NaNtEqual : ResoniteBugFixMonkey<NaNtEqual>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueEqualityDriver<double>), nameof(ValueEqualityDriver<double>.OnChanges))]
        private static bool ValueEqualityDriverDoublePrefix(ValueEqualityDriver<double> __instance)
        {
            if (!Enabled)
                return true;

            if (!__instance.Target.IsLinkValid)
                return false;

            var value = __instance.TargetValue.Target?.Value ?? default;

            var areEqual = (!__instance.UseApproximateComparison.Value || !Coder<double>.SupportsApproximateComparison)
                ? value == __instance.Reference.Value
                : Coder<double>.Approximately(value, __instance.Reference.Value);

            // XOR inverts the other bool
            __instance.Target.Target.Value = __instance.Invert.Value ^ areEqual;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueEqualityDriver<float>), nameof(ValueEqualityDriver<float>.OnChanges))]
        private static bool ValueEqualityDriverFloatPrefix(ValueEqualityDriver<float> __instance)
        {
            if (!Enabled)
                return true;

            if (!__instance.Target.IsLinkValid)
                return false;

            var value = __instance.TargetValue.Target?.Value ?? default;

            var areEqual = (!__instance.UseApproximateComparison.Value || !Coder<float>.SupportsApproximateComparison)
                ? value == __instance.Reference.Value
                : Coder<float>.Approximately(value, __instance.Reference.Value);

            // XOR inverts the other bool
            __instance.Target.Target.Value = __instance.Invert.Value ^ areEqual;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueEquals<double>), nameof(ValueEquals<double>.Compute))]
        private static bool ValueEqualsDoublePrefix(ExecutionContext context, ref bool __result)
        {
            if (!Enabled)
                return true;

            __result = 0.ReadValue<double>(context) == 1.ReadValue<double>(context);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueEquals<float>), nameof(ValueEquals<float>.Compute))]
        private static bool ValueEqualsFloatPrefix(ExecutionContext context, ref bool __result)
        {
            if (!Enabled)
                return true;

            __result = 0.ReadValue<float>(context) == 1.ReadValue<float>(context);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueNotEquals<double>), nameof(ValueNotEquals<double>.Compute))]
        private static bool ValueNotEqualsDoublePrefix(ExecutionContext context, ref bool __result)
        {
            if (!Enabled)
                return true;

            __result = 0.ReadValue<double>(context) != 1.ReadValue<double>(context);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ValueNotEquals<float>), nameof(ValueNotEquals<float>.Compute))]
        private static bool ValueNotEqualsFloatPrefix(ExecutionContext context, ref bool __result)
        {
            if (!Enabled)
                return true;

            __result = 0.ReadValue<float>(context) != 1.ReadValue<float>(context);

            return false;
        }
    }
}