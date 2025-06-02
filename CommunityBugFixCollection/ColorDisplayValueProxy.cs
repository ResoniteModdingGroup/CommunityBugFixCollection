using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    // Todo: do this for strings and objects too?
    [HarmonyPatchCategory(nameof(ColorDisplayValueProxy))]
    [HarmonyPatch(typeof(ValueDisplay<color>), nameof(ValueDisplay<color>.BuildContentUI))]
    internal sealed class ColorDisplayValueProxy : ResoniteBugFixMonkey<ColorDisplayValueProxy>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static void Postfix(ValueDisplay<color> __instance, ProtoFluxNodeVisual visual)
        {
            if (!Enabled)
                return;

            var colorToColorX = visual.Slot.GetComponentInChildren<ColorToColorX>();
            var textFormatDriver = visual.Slot.GetComponentInChildren<MultiValueTextFormatDriver>();

            if (colorToColorX is null || textFormatDriver is null)
                return;

            var valueProxySource = textFormatDriver.Slot.AttachComponent<ValueProxySource<color>>();
            valueProxySource.Value.DriveFrom(__instance._value.Target);
        }
    }
}