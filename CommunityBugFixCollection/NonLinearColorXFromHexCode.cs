using Elements.Core;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(NonLinearColorXFromHexCode))]
    [HarmonyPatch(typeof(colorX), nameof(colorX.FromHexCode), [typeof(string), typeof(colorX), typeof(ColorProfile)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    internal sealed class NonLinearColorXFromHexCode : ResoniteMonkey<NonLinearColorXFromHexCode>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void Prefix(ref ColorProfile profile)
        {
            if (!Enabled)
                return;

            profile = ColorProfile.sRGB;
        }
    }
}