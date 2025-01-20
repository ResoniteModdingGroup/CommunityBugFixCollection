using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(ImportWebFilesAsUrls))]
    [HarmonyPatch(typeof(AssetHelper), nameof(AssetHelper.IdentifyClass))]
    internal sealed class ImportWebFilesAsUrls : ResoniteMonkey<ImportWebFilesAsUrls>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static bool Prefix(out AssetClass __result, string path)
        {
            if (path is null)
            {
                __result = AssetClass.Unknown;
                return false;
            }

            if (Directory.Exists(path))
            {
                __result = AssetClass.Folder;
                return false;
            }

            try
            {
                if (!Uri.TryCreate(path, UriKind.Absolute, out var result))
                {
                    if (Path.IsPathRooted(path))
                    {
                        __result = AssetHelper.ClassifyExtension(Path.GetExtension(path));
                        return false;
                    }

                    __result = AssetClass.Unknown;
                    return false;
                }

                if (AssetHelper.IsVideoStreamingService(result) || AssetHelper.IsStreamingProtocol(result))
                {
                    __result = AssetClass.Video;
                    return false;
                }

                __result = AssetHelper.ClassifyExtension(Path.GetExtension(result.LocalPath));

                if (__result is not AssetClass.Unknown and not AssetClass.Package and not AssetClass.Text)
                    return false;

                if (!string.IsNullOrEmpty(result.Query))
                {
                    foreach (KeyValuePair<string, string> item in StringHelper.ParseQueryString(result.Query))
                    {
                        __result = AssetHelper.ClassifyExtension(Path.GetExtension(item.Value));

                        if (__result != AssetClass.Unknown)
                            return false;
                    }
                }
            }
            catch
            { }

            __result = AssetClass.Unknown;
            return false;
        }

        private static bool Prepare() => Enabled;
    }
}