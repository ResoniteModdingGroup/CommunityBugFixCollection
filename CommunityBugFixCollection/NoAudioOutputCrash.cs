using FrooxEngine;
using HarmonyLib;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(NoAudioOutputCrash))]
    [HarmonyPatch(typeof(AudioOutput), nameof(AudioOutput.UpdateNativeOutput))]
    internal sealed class NoAudioOutputCrash : ResoniteBugFixMonkey<NoAudioOutputCrash>
    {
        public override IEnumerable<string> Authors => Contributors.Nytra;

        private static bool Prefix(AudioOutput __instance)
        {
            if (__instance.Slot.Parent.FilterWorldElement() is not null)
                return true;

            __instance._updateRegistered = false;
            return false;
        }
    }
}