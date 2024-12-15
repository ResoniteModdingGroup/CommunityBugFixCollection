using FrooxEngine.ProtoFlux;
using HarmonyLib;
using MonkeyLoader.Resonite;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FixLeakyImpulseBucketOverflow))]
    [HarmonyPatch(typeof(LocalLeakyImpulseBucket), nameof(LocalLeakyImpulseBucket.DoTrigger))]
    internal sealed class FixLeakyImpulseBucketOverflow : ResoniteMonkey<FixLeakyImpulseBucketOverflow>
    {
        public override bool CanBeDisabled => true;

        private static bool Prepare() => Enabled;

        private static void Prefix(LocalLeakyImpulseBucket __instance, FrooxEngineContext context)
        {
            Logger.Debug(() => $"Capacity: {__instance._capacity.Access(context)}");
            Logger.Debug(() => $"Max. Capacity: {__instance.MaximumCapacity.Evaluate(context, int.MaxValue)}");

            Logger.Debug(() => $"Delay Running: {__instance._delayRunning.Access(context)}");

            var lastImpulse = __instance._lastPulse.Access(context);
            Logger.Debug(() => $"Last Impulse: {context.Time.WorldTime - lastImpulse}s ago ({lastImpulse})");

            Logger.Debug(() => __instance.Pulse.Target);
            Logger.Debug(() => __instance.Overflow.Target);
        }
    }
}
