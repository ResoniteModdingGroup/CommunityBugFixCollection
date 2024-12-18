using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(CorrectMaterialGizmoScaling))]
    [HarmonyPatch(typeof(MaterialGizmo), nameof(MaterialGizmo.PositionInFrontOfUser))]
    internal sealed class CorrectMaterialGizmoScaling : ResoniteMonkey<CorrectMaterialGizmoScaling>
    {
        public override bool CanBeDisabled => true;

        // Apply default scale for inspector UI before it gets user-scaled again by PositionInFrontOfUser
        private static void Prefix(MaterialGizmo __instance)
            => __instance._inspectorRoot.Target.LocalScale = 0.0005f * float3.One;

        private static bool Prepare() => Enabled;
    }
}