using Elements.Core;
using HarmonyLib;
using Renderite.Shared;
using System.Diagnostics.CodeAnalysis;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(NonLinearColorXFromHexCode))]
    [HarmonyPatch(typeof(colorX), nameof(colorX.FromHexCode), [typeof(string), typeof(colorX), typeof(ColorProfile)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal])]
    internal sealed class NonLinearColorXFromHexCode : ResoniteBugFixMonkey<NonLinearColorXFromHexCode>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static void Prefix(ref ColorProfile profile)
        {
            if (!Enabled)
                return;

            profile = ColorProfile.sRGB;
        }
    }
}