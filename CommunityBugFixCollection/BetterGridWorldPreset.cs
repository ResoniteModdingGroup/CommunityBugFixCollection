using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(BetterGridWorldPreset))]
    [HarmonyPatch(typeof(WorldPresets), nameof(WorldPresets.Grid))]
    internal sealed class BetterGridWorldPreset : ResoniteMonkey<BetterGridWorldPreset>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static bool Prefix(World w)
        {
            if (!Enabled)
                return true;

            WorldPresets.BlankWorld(w);

            var ground = w.AddSlot("Ground");
            ground.LocalRotation = floatQ.Euler(90, 0, 0);

            var attachedModel = ground.AttachMesh<GridMesh, PBS_Metallic>();
            attachedModel.mesh.Size.Value = 1000 * float2.One;
            attachedModel.mesh.Points.Value = 20 * int2.One;

            var gridTexture = ground.AttachComponent<GridTexture>();
            gridTexture.BackgroundColor.Value = new colorX(0.07f, 0.08f, 0.11f);
            gridTexture.FilterMode.Value = TextureFilterMode.Anisotropic;
            gridTexture.Size.Value = 256 * int2.One;
            gridTexture.Mipmaps.Value = true;
            gridTexture.Grids.Clear();

            var minorGrid = gridTexture.Grids.Add();
            minorGrid.LineColor.Value = new colorX(0.17f, 0.18f, 0.21f);
            minorGrid.Spacing.Value = 85 * int2.One;
            minorGrid.Width.Value = 2 * int2.One;
            minorGrid.Offset.Value = int2.Zero;

            var majorGrid = gridTexture.Grids.Add();
            majorGrid.LineColor.Value = new colorX(0.88f);
            majorGrid.Spacing.Value = 256 * int2.One;
            majorGrid.Width.Value = 2 * int2.One;
            majorGrid.Offset.Value = int2.Zero;

            attachedModel.material.AlbedoTexture.Target = gridTexture;
            attachedModel.material.TextureScale.DriveFrom(attachedModel.mesh.Size);
            attachedModel.material.Smoothness.Value = 0f;

            var boxCollider = ground.AttachComponent<BoxCollider>();
            var swizzle = ground.AttachComponent<Float2ToFloat3SwizzleDriver>();
            swizzle.Source.Target = attachedModel.mesh.Size;
            swizzle.Target.Target = boxCollider.Size;
            boxCollider.SetCharacterCollider();

            return false;
        }
    }
}