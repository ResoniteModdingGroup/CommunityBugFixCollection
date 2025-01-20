using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ColorDisplayValueProxy))]
    [HarmonyPatch(typeof(ValueDisplay<color>), nameof(ValueDisplay<color>.BuildContentUI))]
    internal sealed class ColorDisplayValueProxy : ResoniteMonkey<ColorDisplayValueProxy>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(ValueDisplay<color> __instance, ProtoFluxNodeVisual visual)
        {
            var colorToColorX = visual.Slot.GetComponentInChildren<ColorToColorX>();
            var textFormatDriver = visual.Slot.GetComponentInChildren<MultiValueTextFormatDriver>();

            if (colorToColorX is null || textFormatDriver is null)
                return;

            var valueProxySource = textFormatDriver.Slot.AttachComponent<ValueProxySource<color>>();
            valueProxySource.Value.DriveFrom(__instance._value.Target);
        }

        private static bool Prepare() => Enabled;
    }
}