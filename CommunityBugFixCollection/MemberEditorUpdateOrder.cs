using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;

// This originally came from yosh (https://git.unix.dog/yosh)
// But I can't find the mod anywhere anymore?

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(MemberEditorUpdateOrder))]
    [HarmonyPatch(typeof(MemberEditor), nameof(MemberEditor.Setup))]
    internal sealed class MemberEditorUpdateOrder : ResoniteMonkey<MemberEditorUpdateOrder>
    {
        public override IEnumerable<string> Authors => Contributors.yosh;

        public override bool CanBeDisabled => true;

        private static void Postfix(MemberEditor __instance)
        {
            if (Enabled)
                __instance.UpdateOrder = int.MaxValue;
        }
    }
}