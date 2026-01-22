using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.Inspectors;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(ChildParentAudioClipPlayer))]
    [HarmonyPatchCategory(nameof(NoParentUnderSelfAudioClipPlayer))]
    internal sealed class NoParentUnderSelfAudioClipPlayer : ResoniteEventHandlerMonkey<NoParentUnderSelfAudioClipPlayer, ResolveInspectorHeaderTextEvent>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Low;

        protected override void Handle(ResolveInspectorHeaderTextEvent eventData)
            => eventData.AddItem(new(Mod.GetLocaleString("ChildParentAudioClipPlayer.Header")));

        private static User? GetAllocatingUser(ChildParentAudioClipPlayer element)
        {
            if (element.FilterWorldElement() is null)
                return null;

            element.ReferenceID.ExtractIDs(out var position, out var allocationId);
            var user = element.World.GetUserByAllocationID(allocationId);

            if (user is null || position < user.AllocationIDStart)
                return null;

            return user;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ChildParentAudioClipPlayer.OnAwake))]
        private static void OnAwakePostfix(ChildParentAudioClipPlayer __instance)
        {
            if (__instance.ParentUnder.Target == __instance.Slot)
            {
                Logger.Warn(() => $"User [{GetAllocatingUser(__instance)}] tried loading a ChildParentAudioClipPlayer targeting itself on: {__instance.ParentHierarchyToString()}");

                if (Enabled)
                    __instance.ParentUnder.Target = null!;
            }

            __instance.ParentUnder.OnTargetChange += _ =>
            {
                if (Enabled && __instance.ParentUnder.Target == __instance.Slot)
                    __instance.RunSynchronously(() => __instance.ParentUnder.Target = null!);
            };
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ChildParentAudioClipPlayer.PlayClip))]
        private static void PlayClipPrefix(ChildParentAudioClipPlayer __instance)
        {
            if (Enabled && __instance.World.CanMakeSynchronousChanges && __instance.ParentUnder.Target == __instance.Slot)
                __instance.ParentUnder.Target = null!;
        }
    }
}