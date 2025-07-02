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
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private class Float
        {
            public float Value;

            public Float(float value)
            {
                Value = value;
            }

            public Float()
            {
                Value = 0f;
            }
        }


        private static readonly ConditionalWeakTable<Animator, Float> _hasChangedPlayhead = new();

        private static bool Prefix(Animator __instance)
        {
            if (!Enabled)
                return true;

            __instance._playback.ClipLength = (__instance.Clip.Asset?.Data?.GlobalDuration).GetValueOrDefault();

            if (!__instance._fieldMappersValid)
                __instance.GenerateFieldMappers();


            if (_hasChangedPlayhead.GetOrCreateValue(__instance).Value != __instance._playback.Position)
            {
                var position = __instance.Position;
                _hasChangedPlayhead.GetOrCreateValue(__instance).Value = __instance.Position;

                foreach (var fieldMapper in __instance._fieldMappers)
                    fieldMapper.Set(position);
            }

            return false;
        }
    }

    
}