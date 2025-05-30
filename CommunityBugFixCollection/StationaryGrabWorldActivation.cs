using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

// Originally released under MIT-0 here:
// https://github.com/art0007i/FixGrabWorld

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(StationaryGrabWorldActivation))]
    [HarmonyPatch(typeof(GrabWorldLocomotion), nameof(GrabWorldLocomotion.TryActivate))]
    internal sealed class StationaryGrabWorldActivation : ResoniteBugFixMonkey<StationaryGrabWorldActivation>
    {
        public override IEnumerable<string> Authors => Contributors.Art0007i;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            var directionReferenceGetter = AccessTools.PropertyGetter(typeof(ILocomotionReference), nameof(ILocomotionReference.DirectionReference));

            foreach (var code in codes)
            {
                if (Enabled && code.Calls(directionReferenceGetter))
                    code.operand = AccessTools.PropertyGetter(typeof(ILocomotionReference), nameof(ILocomotionReference.GripReference));

                yield return code;
            }
        }
    }
}