using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(Slot), nameof(Slot.Duplicate))]
    [HarmonyPatchCategory(nameof(CheckSelfForDuplicateSlot))]
    internal sealed class CheckSelfForDuplicateSlot : ResoniteMonkey<CheckSelfForDuplicateSlot>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var isChildOfMethod = AccessTools.DeclaredMethod(typeof(Slot), nameof(Slot.IsChildOf));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(isChildOfMethod))
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                }

                yield return instruction;
            }
        }
    }
}