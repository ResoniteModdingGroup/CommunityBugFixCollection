using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(PauseAnimatorUpdates))]
    [HarmonyPatch(typeof(Animator), nameof(Animator.OnCommonUpdate))]
    internal sealed class PauseAnimatorUpdates : ResoniteBugFixMonkey<PauseAnimatorUpdates>
    {
        private static readonly ConditionalWeakTable<Animator, Box<float>> _lastPositionByAnimator = new();

        public override IEnumerable<string> Authors { get; } = [.. Contributors.Banane9, .. Contributors.Onan];

        private static bool Prefix(Animator __instance)
        {
            if (!Enabled)
                return true;

            __instance._playback.ClipLength = (__instance.Clip.Asset?.Data?.GlobalDuration).GetValueOrDefault();

            if (!__instance._fieldMappersValid)
                __instance.GenerateFieldMappers();

            if (!_lastPositionByAnimator.TryGetValue(__instance, out var lastPosition))
            {
                // Make sure that initial state is always applied,
                // since playback position can't be < 0
                lastPosition = -1;
                _lastPositionByAnimator.Add(__instance, lastPosition);
            }

            if (lastPosition != __instance.Position)
            {
                var position = lastPosition.Value = __instance.Position;

                foreach (var fieldMapper in __instance._fieldMappers)
                    fieldMapper.Set(position);
            }

            return false;
        }
    }
}