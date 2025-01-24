using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(HighlightHomeWorldInInventory))]
    [HarmonyPatch(typeof(InventoryBrowser), nameof(InventoryBrowser.ProcessItem))]
    internal sealed class HighlightHomeWorldInInventory : ResoniteMonkey<HighlightHomeWorldInInventory>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Postfix(InventoryBrowser __instance, InventoryItemUI item)
        {
            if (!Enabled
             || InventoryBrowser.ClassifyItem(item) != InventoryBrowser.SpecialItemType.World
             || __instance.GetItemWorldUri(item) != __instance.Engine.Cloud.Profile.GetCurrentFavorite(FavoriteEntity.Home))
                return;

            item.NormalColor.Value = InventoryBrowser.FAVORITE_COLOR;
            item.SelectedColor.Value = InventoryBrowser.FAVORITE_COLOR.MulRGB(2f);
        }
    }
}