using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

// Originally released under MIT-0 here:
// https://github.com/art0007i/NotificationFixer

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(CenteredNotifications))]
    [HarmonyPatch(typeof(NotificationPanel), nameof(NotificationPanel.OnCommonUpdate))]
    internal class CenteredNotifications : ResoniteBugFixMonkey<CenteredNotifications>
    {
        public override IEnumerable<string> Authors => Contributors.Art0007i;

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                if (Enabled && code.LoadsConstant(36.0f))
                    code.operand = 32.0f;

                yield return code;
            }
        }
    }
}