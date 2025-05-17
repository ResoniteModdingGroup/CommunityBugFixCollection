using FrooxEngine;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ReallyNoDestroyUndo))]
    [HarmonyPatch(typeof(SpawnOrDestroyExtensions), nameof(SpawnOrDestroyExtensions.NoDestroyUndo))]
    internal sealed class ReallyNoDestroyUndo : ResoniteMonkey<ReallyNoDestroyUndo>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static bool Postfix(bool __result, Slot slot)
        {
            if (!Enabled)
                return __result;

            return __result || slot.GetComponent<NoDestroyUndo>() is not null;
        }
    }
}