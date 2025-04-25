using HarmonyLib;
using MonkeyLoader.Resonite;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes.Casts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommunityBugFixCollection
{
    internal sealed class CorrectByteCasting : ResoniteMonkey<CorrectByteCasting>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        protected override bool OnEngineReady()
        {
            var targets = AccessTools.GetTypesFromAssembly(typeof(Cast_byte_To_int).Assembly)
                .Select(type => (Type: type, ValueCast: type.GetCustomAttribute<ValueCastAttribute>()))
                .Where(candidate => candidate.ValueCast?.From == typeof(byte))
                .Select(candidate => (candidate.ValueCast, Method: AccessTools.DeclaredMethod(candidate.Type, nameof(Cast_byte_To_int.Compute))))
                .Where(target => target.Method is not null);

            var computePrefix = AccessTools.DeclaredMethod(typeof(CorrectByteCasting), nameof(ComputePrefix));

            foreach (var target in targets)
                Harmony.Patch(target.Method, computePrefix.MakeGenericMethod(target.ValueCast.To));

            return true;
        }

        private static bool ComputePrefix<T>(ExecutionContext context, ref T __result)
            where T : unmanaged
        {
            if (!Enabled)
                return true;

            var value = (object)0.ReadValue<byte>(context);
            __result = (T)(Convert.ChangeType(value, typeof(T)) ?? default(T));

            return false;
        }
    }
}