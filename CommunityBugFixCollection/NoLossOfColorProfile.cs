using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(colorX))]
    [HarmonyPatchCategory(nameof(NoLossOfColorProfile))]
    internal sealed class NoLossOfColorProfile : ResoniteMonkey<NoLossOfColorProfile>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(colorX.MulA))]
        private static bool MulAPrefix(colorX __instance, float a, out colorX __result)
        {
            __result = __instance.MultiplyA(a);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(colorX.NormalizeHDR))]
        private static colorX NormalizeHDRPostfix(colorX __result, colorX __instance)
            => new(__result.BaseColor, __instance.Profile);

        private static bool Prepare() => Enabled;

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(NoLossOfColorProfile))]
        private static class ColorXBlendingPatches
        {
            private static colorX Postfix(colorX __result, ColorProfile __state)
            {
                if (!Enabled)
                    return __result;

                return new(__result.BaseColor, __state);
            }

            private static void Prefix(ref colorX src, ref colorX dst, out ColorProfile __state)
            {
                if (!Enabled)
                {
                    __state = default;
                    return;
                }

                var operands = ColorProfileHelper.GetOperands(in src, in dst, ColorProfileAwareOperation.LinearIfUnequal);
                src = new(operands.leftHand, operands.profile);
                dst = new(operands.rightHand, operands.profile);
                __state = operands.profile;
            }

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methodNames = new[]
                {
                    nameof(colorX.AlphaBlend), nameof(colorX.AdditiveBlend), nameof(colorX.SoftAdditiveBlend)
                };

                return methodNames
                    .Select(name => AccessTools.DeclaredMethod(typeof(colorX), name))
                    .Where(method => method is not null);
            }
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(NoLossOfColorProfile))]
        private static class ColorXChannelAddingPatches
        {
            private static colorX Postfix(colorX __result, colorX __instance)
            {
                if (!Enabled)
                    return __result;

                return new(__result.BaseColor, __instance.Profile);
            }

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methodNames = new[]
                {
                    nameof(colorX.AddR), nameof(colorX.AddR_HDR),
                    nameof(colorX.AddG), nameof(colorX.AddG_HDR),
                    nameof(colorX.AddB), nameof(colorX.AddB_HDR),
                    nameof(colorX.AddA)
                };

                return methodNames
                    .Select(name => AccessTools.DeclaredMethod(typeof(colorX), name))
                    .Where(method => method is not null);
            }
        }
    }
}