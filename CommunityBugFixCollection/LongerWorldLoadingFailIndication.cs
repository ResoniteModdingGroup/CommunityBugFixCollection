using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(LongerWorldLoadingFailIndication))]
    [HarmonyPatch(typeof(WorldLoadProgress), nameof(WorldLoadProgress.OnCommonUpdate))]
    internal sealed class LongerWorldLoadingFailIndication : ResoniteBugFixMonkey<LongerWorldLoadingFailIndication>
    {
        private static float _vanillaDelay = 5f;
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static float GetDestroyDelay()
            => Enabled ? Math.Max(20f, _vanillaDelay) : _vanillaDelay;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var instructions = new List<CodeInstruction>(codeInstructions);

            var getDestroyDelayMethod = AccessTools.DeclaredMethod(typeof(LongerWorldLoadingFailIndication), nameof(GetDestroyDelay));
            var runInSecondsMethod = AccessTools.DeclaredMethod(typeof(ComponentBase<Component>), nameof(ComponentBase<Component>.RunInSeconds));

            var runInSecondsIndex = instructions.FindIndex(instruction => instruction.Calls(runInSecondsMethod));
            var loadDelayIndex = instructions.FindLastIndex(runInSecondsIndex, instruction => instruction.opcode == OpCodes.Ldc_R4);

            _vanillaDelay = (float)instructions[loadDelayIndex].operand;
            instructions[loadDelayIndex] = new CodeInstruction(OpCodes.Call, getDestroyDelayMethod);

            return instructions;
        }
    }
}