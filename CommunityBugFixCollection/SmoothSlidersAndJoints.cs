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
    [HarmonyPatch("OnAwake")]
    [HarmonyPatchCategory(nameof(SmoothSlidersAndJoints))]
    internal sealed class SmoothSlidersAndJoints : ResoniteMonkey<SmoothSlidersAndJoints>
    {
        public override IEnumerable<string> Authors => base.Authors;
        public override bool CanBeDisabled => true;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Joint))]
        public static void JointOnAwakePostfix(Joint __instance)
        {
            if (!Enabled || __instance.LocalUser.HeadDevice != HeadOutputDevice.Headless || !__instance.LocalUser.IsHost)
                return;

            __instance.RunInUpdates(3, () =>
            {
                if (__instance.FilterWorldElement() is null)
                    return;

                __instance.DontDrive.Value = true;

                Logger.Info(() => $"Set DontDrive to true for Joint:");
                Logger.Info(() => __instance.ParentHierarchyToString());
            });
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Slider))]
        public static void SliderOnAwakePostfix(Slider __instance)
        {
            if (!Enabled || __instance.LocalUser.HeadDevice != HeadOutputDevice.Headless || !__instance.LocalUser.IsHost)
                return;

            __instance.RunInUpdates(3, () =>
            {
                if (__instance.FilterWorldElement() is null)
                    return;

                __instance.DontDrive.Value = true;

                Logger.Info(() => $"Set DontDrive to true for Slider:");
                Logger.Info(() => __instance.ParentHierarchyToString());
            });
        }
    }
}