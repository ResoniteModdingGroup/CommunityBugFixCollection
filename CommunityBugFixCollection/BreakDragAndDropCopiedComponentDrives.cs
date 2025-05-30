using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Meta;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(BreakDragAndDropCopiedComponentDrives))]
    [HarmonyPatch(typeof(SlotComponentReceiver), nameof(SlotComponentReceiver.TryReceive))]
    internal sealed class BreakDragAndDropCopiedComponentDrives : ResoniteBugFixMonkey<BreakDragAndDropCopiedComponentDrives>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        protected override bool OnEngineReady()
        {
            var integrationMod = Mod.Loader.Get<Mod>().ById("MonkeyLoader.GamePacks.Resonite");

            // Newer version than last one that did not include the fix
            if (integrationMod is not null && integrationMod.Version > new NuGetVersion(0, 22, 1))
            {
                Logger.Info(() => "Skipping in favor of the Resonite Integration fix.");
                return false;
            }

            return base.OnEngineReady();
        }

        private static bool Prefix(SlotComponentReceiver __instance, IEnumerable<IGrabbable> items, Canvas.InteractionData eventData, out bool __result)
        {
            __result = false;

            if (__instance.Target.Target is null)
                return false;

            foreach (var item in items)
            {
                foreach (ReferenceProxy componentsInChild in item.Slot.GetComponentsInChildren<ReferenceProxy>())
                {
                    if (componentsInChild.Reference.Target is not Component component || __instance.Target.Target == component.Slot)
                        continue;

                    __instance.StartTask(async () =>
                    {
                        var contextMenu = await __instance.LocalUser.OpenContextMenu(__instance, eventData.source.Slot);

                        var copyItem = contextMenu.AddItem("Inspector.Actions.CopyComponent".AsLocaleKey(), (Uri)null!, RadiantUI_Constants.Hero.GREEN);
                        var moveItem = contextMenu.AddItem("Inspector.Actions.MoveComponent".AsLocaleKey(), (Uri)null!, RadiantUI_Constants.Hero.PURPLE);
                        var cancelItem = contextMenu.AddItem("General.Cancel".AsLocaleKey(), (Uri)null!, colorX.White);

                        copyItem.Button.LocalPressed += delegate
                        {
                            __instance.Target.Target.DuplicateComponent(component);
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };

                        moveItem.Button.LocalPressed += delegate
                        {
                            __instance.Target.Target.MoveComponent(component);
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };

                        cancelItem.Button.LocalPressed += delegate
                        {
                            __instance.LocalUser.CloseContextMenu(__instance);
                        };
                    });

                    __result = true;
                    return false;
                }
            }

            return false;
        }
    }
}