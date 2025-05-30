using FrooxEngine;
using HarmonyLib;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.Text;

// Initially done by https://github.com/goaaats

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(GroupCloudHomeTemplate))]
    [HarmonyPatch(typeof(Userspace), nameof(Userspace.GetHomeTemplateUri))]
    internal sealed class GroupCloudHomeTemplate : ResoniteBugFixMonkey<GroupCloudHomeTemplate>
    {
        public override IEnumerable<string> Authors => Contributors.Goat;

        private static void Prefix(ref OwnerType ownerType)
        {
            // The template URL for group homes currently does not resolve.
            // We change the owner type to User, so that we can create a group home with the standard cloud home template.
            if (Enabled && ownerType is OwnerType.Group)
                ownerType = OwnerType.User;
        }
    }
}