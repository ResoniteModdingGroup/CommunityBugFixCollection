using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Text;

// Originally released under MIT-0 here:
// https://github.com/art0007i/FixMigratedItemMessages

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FixMigratedItemMessages))]
    [HarmonyPatch(typeof(ContactsDialog), nameof(ContactsDialog.SpawnMessageItem))]
    internal sealed class FixMigratedItemMessages : ResoniteMonkey<FixMigratedItemMessages>
    {
        private const string OldPrefix = "neosdb";

        public override IEnumerable<string> Authors => Contributors.Art0007i;

        public override bool CanBeDisabled => true;

        public static void Prefix(ref Record record)
        {
            if (Enabled && record.AssetURI.StartsWith(OldPrefix))
                record.AssetURI = $"{Engine.Current.PlatformProfile.DBScheme}{record.AssetURI[OldPrefix.Length..]}";
        }
    }
}