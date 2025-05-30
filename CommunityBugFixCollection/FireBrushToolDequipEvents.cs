using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FireBrushToolDequipEvents))]
    [HarmonyPatch(nameof(BrushTool.OnDequipped))]
    internal sealed class FireBrushToolDequipEvents : ResoniteBugFixMonkey<FireBrushToolDequipEvents>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BrushTool))]
        private static IEnumerable<CodeInstruction> BrushToolOnDequippedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new[]
            {
                CodeInstruction.LoadArgument(0),
                CodeInstruction.Call((Tool tool) => ToolOnDequippedProxy(tool))
            }
            .Concat(instructions);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Tool))]
        private static void ToolOnDequipped(Tool tool)
            => throw new NotImplementedException("Harmony Reverse Patch!");

        /// <summary>
        /// Proxy to be able to control the fix through <see cref="MonkeyBase{T}.Enabled"/>.
        /// </summary>
        private static void ToolOnDequippedProxy(Tool tool)
        {
            if (!Enabled)
                return;

            ToolOnDequipped(tool);
        }
    }
}