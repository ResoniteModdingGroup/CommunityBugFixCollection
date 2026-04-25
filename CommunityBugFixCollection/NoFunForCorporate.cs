using FrooxEngine;
using HarmonyLib;

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(NoFunForCorporate))]
    internal sealed class NoFunForCorporate : ResoniteBugFixMonkey<NoFunForCorporate>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Engine), nameof(Engine.IsAprilFools), MethodType.Getter)]
        private static bool EngineIsAprilFoolsPrefix(ref bool __result)
        {
            if (!Enabled)
                return true;

            if (ConfigSection.ForceAprilFools)
            {
                __result = true;
                return false;
            }

            if (Engine.Current.InUniverse)
            {
                __result = false;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ViolentAprilFoolsExplosion), nameof(ViolentAprilFoolsExplosion.OnDestroying))]
        private static bool ViolentAprilFoolsExplosionOnDestroyingPrefix()
            => !Enabled || ConfigSection.ForceAprilFools || !Engine.Current.InUniverse;
    }
}