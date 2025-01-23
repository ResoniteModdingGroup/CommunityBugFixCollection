using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

// Originally released under MIT by LeCloutPanda here:
// https://github.com/LeCloutPanda/SoNoHeadCrash

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(SmoothDraggables))]
    [HarmonyPatch(typeof(Draggable), nameof(Draggable.OnAwake))]
    internal sealed class SmoothDraggables : ResoniteMonkey<SmoothDraggables>
    {
        public override IEnumerable<string> Authors => base.Authors;

        public override bool CanBeDisabled => true;

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