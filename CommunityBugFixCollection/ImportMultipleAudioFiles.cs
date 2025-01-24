using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.Store;
using FrooxEngine.Undo;
using HarmonyLib;
using MonkeyLoader.Resonite;
using SkyFrost.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ImportMultipleAudioFiles))]
    [HarmonyPatch(typeof(UniversalImporter), nameof(UniversalImporter.ImportTask))]
    internal sealed class ImportMultipleAudioFiles : ResoniteMonkey<ImportMultipleAudioFiles>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static async Task ImportAudioAsync(IEnumerable<string> files, World world, float3 position, floatQ rotation, float3 scale)
        {
            await default(ToWorld);

            if (!world.CanSpawnObjects())
                return;

            var list = files.Where(s => s is not null)
                .Select(s => s.Trim())
                .ToList();

            var count = list.Count;
            var rowSize = MathX.Max(1, MathX.CeilToInt(MathX.Sqrt(count)));
            var index = 0;

            UniLog.Log($"Importing files with Asset Class {AssetClass.Audio}:\n" + string.Join("\n", list));

            foreach (var file in list)
            {
                await default(ToWorld);

                var slot = world.LocalUserSpace.AddSlot(Path.GetFileName(file));
                slot.CreateSpawnUndoPoint();

                var gridOffset = UniversalImporter.GridOffset(ref index, rowSize);
                var direction = rotation * gridOffset;
                var offset = direction * scale;

                slot.GlobalPosition = position + offset;
                slot.GlobalRotation = rotation;
                slot.GlobalScale = scale;

                await default(ToBackground);

                Uri uri;
                if (File.Exists(file))
                    uri = await world.Engine.LocalDB.ImportLocalAssetAsync(file, LocalDB.ImportLocation.Original);
                else if (!Uri.TryCreate(file, UriKind.Absolute, out uri))
                    continue;

                await default(ToWorld);

                var audioPlayerInterface = await slot.SpawnEntity<AudioPlayerInterface, LegacyAudioPlayer>(FavoriteEntity.AudioPlayer);
                audioPlayerInterface.InitializeEntity(Path.GetFileName(file));
                audioPlayerInterface.SetSource(uri);

                if (".wav".Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                {
                    audioPlayerInterface.SetType(AudioTypeGroup.SoundEffect, spatialize: true, 1f);
                    continue;
                }

                var audioTypeGroup = await UniversalImporter.DetectAudioTypeGroup(audioPlayerInterface.Clip.Target.Target);

                var isMultimedia = audioTypeGroup == AudioTypeGroup.Multimedia;
                audioPlayerInterface.SetType(audioTypeGroup, isMultimedia, isMultimedia ? 1 : 0);
            }
        }

        private static bool Prefix(ref Task? __result, AssetClass assetClass, IEnumerable<string> files, World world, float3 position, floatQ rotation, float3 scale)
        {
            if (!Enabled || assetClass != AssetClass.Audio)
                return true;

            __result = ImportAudioAsync(files, world, position, rotation, scale);
            return false;
        }
    }
}