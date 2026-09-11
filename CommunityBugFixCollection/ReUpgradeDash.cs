using FrooxEngine;

namespace CommunityBugFixCollection
{
    internal sealed class ReUpgradeDash : ResoniteBugFixMonkey<ReUpgradeDash>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        protected override bool OnComputeDefaultEnabledState()
            => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            var userspaceDash = Userspace.Current.World
                .GetGloballyRegisteredComponent<UserspaceRadiantDash>();

            userspaceDash.Slot.ForeachComponentInChildren<RadiantDashScreen>(
                dashScreen => dashScreen.CleanupOldComponents(RadiantDashScreen.DASH_SCREENS_CLEANUP_VERSION),
                cacheItems: true);

            Enabled = false;
        }
    }
}