using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FlipAtUserView))]
    [HarmonyPatch(typeof(FlipAtUser), nameof(FlipAtUser.OnCommonUpdate))]
    internal sealed class FlipAtUserView : ResoniteBugFixMonkey<FlipAtUserView>
    {
        public override IEnumerable<string> Authors => Contributors.E1int;

        private static float3 GetUserPosition(UserRoot userRoot)
            => Enabled ? userRoot.ViewPosition : userRoot.HeadPosition;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var headPositionGetter = AccessTools.PropertyGetter(typeof(UserRoot), nameof(UserRoot.HeadPosition));
            var getUserPositionMethod = AccessTools.DeclaredMethod(typeof(FlipAtUserView), nameof(GetUserPosition));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(headPositionGetter))
                {
                    yield return new CodeInstruction(OpCodes.Call, getUserPositionMethod);
                    continue;
                }

                yield return instruction;
            }
        }
    }
}