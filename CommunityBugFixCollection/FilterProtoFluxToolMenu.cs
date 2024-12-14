using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FilterProtoFluxToolMenu))]
    [HarmonyPatch(typeof(ProtoFluxTool), nameof(ProtoFluxTool.GenerateMenuItems)]
    internal sealed class FilterProtoFluxToolMenu : ResoniteMonkey<FilterProtoFluxToolMenu>
    {
        public override bool CanBeDisabled => true;

        private static bool Prepare() => Enabled;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var slotType = typeof(Slot);

            var instructions = new List<CodeInstruction>(codeInstructions);
            var slotCheckIndex = instructions.FindIndex(instruction => instruction.Is(OpCodes.Isinst, slotType));

            var branchTarget = 
            var branchIndex = instructions.FindIndex(slotCheckIndex, instruction => instruction.Branches())
        }
    }
}
