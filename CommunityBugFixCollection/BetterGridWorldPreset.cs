using Elements.Core;
using FrooxEngine;
using FrooxEngine.FrooxEngine.ProtoFlux.CoreNodes;
using FrooxEngine.ProtoFlux;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes;
using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Operators;
using HarmonyLib;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(BetterGridWorldPreset))]
    [HarmonyPatch(typeof(WorldPresets), nameof(WorldPresets.Grid))]
    internal sealed class BetterGridWorldPreset : ResoniteBugFixMonkey<BetterGridWorldPreset>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

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
            boxCollider.Offset.Value = new(0, 0, 0.5f);
            boxCollider.SetCharacterCollider();

            var swizzleSlot = ground.AddSlot("Collider Swizzle");

            var meshSizeSource = swizzleSlot.AttachComponent<ValueSource<float2>>();
            meshSizeSource.TrySetRootSource(attachedModel.mesh.Size);

            var unpackSize = swizzleSlot.AttachComponent<Unpack_Float2>();
            unpackSize.V.Target = meshSizeSource;

            var oneConstant = swizzleSlot.AttachComponent<ValueInput<float>>();
            oneConstant.Value.Value = 1;

            var packSize = swizzleSlot.AttachComponent<Pack_Float3>();
            packSize.X.Target = unpackSize.X;
            packSize.Y.Target = unpackSize.Y;
            packSize.Z.Target = oneConstant;

            var colliderSizeDrive = swizzleSlot.AttachComponent<ValueFieldDrive<float3>>();
            colliderSizeDrive.TrySetRootTarget(boxCollider.Size);
            colliderSizeDrive.Value.Target = packSize;

            return false;
        }
    }
}