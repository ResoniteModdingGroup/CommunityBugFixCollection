using FrooxEngine;
using HarmonyLib;
using SkyFrost.Base;

// Originally released under MIT-0 here:
// https://github.com/art0007i/FixMigratedItemMessages

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FixMigratedItemMessages))]
    [HarmonyPatch(typeof(ContactsDialog), nameof(ContactsDialog.SpawnMessageItem))]
    internal sealed class FixMigratedItemMessages : ResoniteBugFixMonkey<FixMigratedItemMessages>
    {
        private const string OldPrefix = "neosdb";

        public override IEnumerable<string> Authors => Contributors.Art0007i;

        public static void Prefix(Record record)
        {
            if (Enabled && record.AssetURI.StartsWith(OldPrefix))
                record.AssetURI = $"{Engine.Current.PlatformProfile.DBScheme}{record.AssetURI[OldPrefix.Length..]}";
        }
    }
}