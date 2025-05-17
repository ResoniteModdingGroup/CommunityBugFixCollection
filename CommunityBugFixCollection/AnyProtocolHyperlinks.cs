using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(AnyProtocolHyperlinks))]
    internal sealed class AnyProtocolHyperlinks : ResoniteMonkey<AnyProtocolHyperlinks>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HyperlinkOpenDialog), nameof(HyperlinkOpenDialog.Open))]
        private static bool HyperlinkOpenDialogOpenPrefix(HyperlinkOpenDialog __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.World != Userspace.UserspaceWorld)
                return false;

            if (__instance.URL.Value is not null)
            {
                Logger.Debug(() => $"Opening Hyperlink: {__instance.URL.Value}");
                __instance.RunInBackground(() => Process.Start(__instance.URL.Value.ToString()));
            }

            __instance.Slot.Destroy();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hyperlink), nameof(Hyperlink.Open))]
        private static bool HyperlinkOpenPostfix(bool __result, Hyperlink __instance)
        {
            if (!Enabled)
                return __result;

            if (__result)
                return true;

            var url = __instance.URL.Value;
            if (url is null || (__instance.OpenOnce.Value && url == __instance._lastOpened))
                return false;

            __instance._lastOpened = url;

            Userspace.UserspaceWorld.RunSynchronously(delegate
            {
                Slot slot = Userspace.UserspaceWorld.AddSlot("Hyperlink");
                slot.PositionInFrontOfUser(float3.Backward);
                slot.AttachComponent<HyperlinkOpenDialog>().Setup(url, __instance.Reason.Value);
            });

            __instance.RunInSeconds(5f, () => __instance._lastOpened = null!);

            return true;
        }
    }
}