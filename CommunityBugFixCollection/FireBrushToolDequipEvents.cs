using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FireBrushToolDequipEvents))]
    [HarmonyPatch(typeof(BrushTool), nameof(BrushTool.OnDequipped))]
    internal sealed class FireBrushToolDequipEvents : ResoniteMonkey<FireBrushToolDequipEvents>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static bool Prepare() => Enabled;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new[]
            {
                CodeInstruction.LoadArgument(0),
                CodeInstruction.Call((Tool tool) => tool.OnDequipped())
            }
            .Concat(instructions);
        }
    }
}