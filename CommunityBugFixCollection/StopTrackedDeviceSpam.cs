using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(StopTrackedDeviceSpam))]
    [HarmonyPatch(typeof(TrackedDevicePositioner), nameof(TrackedDevicePositioner.UpdateBodyNode))]
    internal sealed class StopTrackedDeviceSpam : ResoniteBugFixMonkey<StopTrackedDeviceSpam>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyPatch()]
        private static bool Prefix(TrackedDevicePositioner __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.TrackedDevice is not ITrackedDevice device)
                return false;

            var bodyNode = device.CorrespondingBodyNode;
            if (__instance.ObjectSlot.Target is not null && __instance.UsingExternalObjectSlot)
            {
                if (__instance.ObjectSlot.Target.Node.Value == bodyNode)
                    return false;

                __instance.ObjectSlot.Target = null!;
            }

            var target = __instance.ObjectSlot.Target;
            if (target is null || target.Node.Value != bodyNode)
            {
                var existingBodyNode = __instance.LocalUserRoot.GetRegisteredComponent((AvatarObjectSlot s) => s.Node.Value == bodyNode);

                if (existingBodyNode != null)
                {
                    UniLog.Log($"Device corresponding body node: {bodyNode}. Exiting: {__instance.ObjectSlot.Target?.Node.Value}");

                    __instance.RemoveBodyNode();
                    __instance.ObjectSlot.Target = existingBodyNode;

                    return false;
                }
            }

            if (__instance.CreateAvatarObjectSlot.Value)
                __instance.UpdateObjectSlot();

            if (__instance.BodyNodeRoot.Target.FilterWorldElement() is not null)
            {
                if (device.IsDeviceActive)
                {
                    __instance.BodyNodeRoot.Target.LocalPosition = device.BodyNodePositionOffset;
                    __instance.BodyNodeRoot.Target.LocalRotation = device.BodyNodeRotationOffset;
                }
                else
                {
                    __instance.BodyNodeRoot.Target.LocalPosition = float3.Zero;
                    __instance.BodyNodeRoot.Target.LocalRotation = floatQ.Identity;
                }
            }

            if (__instance.ObjectSlot.Target!.FilterWorldElement() is not null)
                __instance.ObjectSlot.Target!.Node.Value = bodyNode;

            return false;
        }
    }
}