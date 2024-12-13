﻿using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommunityBugFixCollection
{
    internal sealed class NonHDRColorClamping : ResoniteMonkey<NonHDRColorClamping>
    {
        public override bool CanBeDisabled => true;

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(NonHDRColorClamping))]
        private static class ColorXPatches
        {
            private static bool Prepare() => Enabled;

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methodNames = new[]
                {
                    nameof(colorX.AddR), nameof(colorX.AddG), nameof(colorX.AddB), nameof(colorX.AddA)
                };

                return methodNames
                    .Select(name => AccessTools.DeclaredMethod(typeof(colorX), name))
                    .Where(method => method is not null);
            }

            private static colorX Postfix(colorX __result)
                => MathX.Clamp01(in __result);
        }

        [HarmonyPatch]
        [HarmonyPatchCategory(nameof(NonHDRColorClamping))]
        private static class ColorPatches
        {
            private static bool Prepare() => Enabled;

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var methodNames = new[]
                {
                    nameof(color.AddR), nameof(color.AddG), nameof(color.AddB), nameof(color.AddA)
                };

                return methodNames
                    .Select(name => AccessTools.DeclaredMethod(typeof(color), name))
                    .Where(method => method is not null);
            }

            private static color Postfix(color __result)
                => new(MathX.Clamp01(__result.rgba));
        }
    }
}
