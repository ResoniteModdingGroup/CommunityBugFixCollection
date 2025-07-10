using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

// Originally released under MIT by LeCloutPanda here:
// https://github.com/LeCloutPanda/SoNoHeadCrash

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(SmoothDraggables))]
    [HarmonyPatch(typeof(Draggable), nameof(Draggable.OnAwake))]
    internal sealed class SmoothDraggables : ResoniteBugFixMonkey<SmoothDraggables>
    {
        public override IEnumerable<string> Authors => Contributors.LeCloutPanda;

        public static void Postfix(Draggable __instance)
        {
            if (!Enabled || __instance.LocalUser.HeadDevice != HeadOutputDevice.Headless || !__instance.LocalUser.IsHost)
                return;

            __instance.RunInUpdates(3, () =>
            {
                if (__instance.FilterWorldElement() is null)
                    return;

                __instance.DontDrive.Value = true;

                Logger.Info(() => $"Set DontDrive to true for Draggable:");
                Logger.Info(() => __instance.ParentHierarchyToString());
            });
        }
    }
}