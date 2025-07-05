using HarmonyLib;
using System.Collections.Generic;

namespace CommunityBugFixCollection
{
    [HarmonyPatch("Valve.VR.SteamVR_Render", "OnInputFocus")]
    [HarmonyPatchCategory(nameof(SteamVRFocusResolutionScale))]
    internal sealed class SteamVRFocusResolutionScale : ResoniteBugFixMonkey<SteamVRFocusResolutionScale>
    {
        // SteamVR_Render is treated as a singleton in SteamVR and initialized once from SteamVRDriver through SteamVR.Initialize()
        private static bool _lastInputFocus = false;

        public override IEnumerable<string> Authors => Contributors.Goat;

        public static bool Prefix(bool hasFocus)
        {
            // Work around some broken logic in SteamVR focus handling
            // https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/2337#issuecomment-3025681468
            // https://github.com/ValveSoftware/steamvr_unity_plugin/blob/056c82369d78f253af8cefcae9b289efd69bd960/Assets/SteamVR/Scripts/SteamVR_Render.cs#L237-L262
            if (Enabled && !hasFocus && !_lastInputFocus)
            {
                Logger.Trace(() => "Dropping redundant OnInputFocus unfocus call!");
                return false;
            }

            _lastInputFocus = hasFocus;
            return true;
        }
    }
}