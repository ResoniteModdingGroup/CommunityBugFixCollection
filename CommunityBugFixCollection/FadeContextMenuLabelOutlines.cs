using Elements.Core;
using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Color;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FadeContextMenuLabelOutlines))]
    [HarmonyPatch(typeof(ContextMenu), nameof(ContextMenu.OnAttach))]
    internal sealed class FadeContextMenuLabelOutlines : ResoniteMonkey<FadeContextMenuLabelOutlines>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(ContextMenu __instance)
        {
            var slot = __instance.Slot;

            // Create protoflux to drive the font materials's OutlineColor in sync with the (white) fade's alpha

            var source = slot.AttachComponent<ValueSource<colorX>>();
            source.TrySetRootSource(__instance._fillFade.Target);

            var setAlpha = slot.AttachComponent<ColorXSetValue>();
            setAlpha.Color.Target = source;

            var drive = slot.AttachComponent<ValueFieldDrive<colorX>>();
            drive.Value.Target = setAlpha;
            drive.TrySetRootTarget(__instance._fontMaterial.Target.OutlineColor);
        }
    }
}