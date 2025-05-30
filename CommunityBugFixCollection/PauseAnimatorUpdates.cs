using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(PauseAnimatorUpdates))]
    [HarmonyPatch(typeof(Animator), nameof(Animator.OnCommonUpdate))]
    internal sealed class PauseAnimatorUpdates : ResoniteBugFixMonkey<PauseAnimatorUpdates>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static bool Prefix(Animator __instance)
        {
            if (!Enabled)
                return true;

            __instance._playback.ClipLength = (__instance.Clip.Asset?.Data?.GlobalDuration).GetValueOrDefault();

            if (!__instance._fieldMappersValid)
                __instance.GenerateFieldMappers();

            if (__instance.IsPlaying)
            {
                var position = __instance.Position;

                foreach (var fieldMapper in __instance._fieldMappers)
                    fieldMapper.Set(position);
            }

            return false;
        }
    }
}