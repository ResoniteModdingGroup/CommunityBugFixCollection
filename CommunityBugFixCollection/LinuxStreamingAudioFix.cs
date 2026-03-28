using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Meta;

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(LinuxStreamingAudioFix))]
    internal sealed class LinuxStreamingAudioFix : ResoniteBugFixMonkey<LinuxStreamingAudioFix>
    {
        private static readonly Version _requiredShowmanToolsVersion = new(0, 2, 0);

        public override IEnumerable<string> Authors => Contributors.Banane9;

        protected override bool OnEngineReady()
        {
            if (Mod.Loader.TryGet<Mod>().ById("ShowmanTools", out var mod) && mod.Version.Version >= _requiredShowmanToolsVersion)
            {
                Logger.Info(() => "Skipping in favor of the ShowmanTools fix.");
                return false;
            }

            return base.OnEngineReady();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSystem), nameof(AudioSystem.UpdateDefaultAudioOutput))]
        private static bool UpdateDefaultAudioOutputPrefix(AudioSystem __instance, Action<AudioOutputDriver>? ___DefaultAudioOutputChanged)
        {
            if (!Enabled)
                return true;

            if (__instance._outputDevice is null)
                return false;

            var preferredDevice = __instance._outputDevice.FindPreferredDevice(d => __instance._audioOutputDeviceIDs.Contains(d.EntryID));
            var preferredStreamingDevice = __instance._outputDevice.FindStreamingPreferredDevice(d => __instance._audioOutputDeviceIDs.Contains(d.EntryID));

            var preferredDeviceID = preferredDevice is not null
                ? (string)preferredDevice.EntryID
                : (__instance.AudioOutputs.FirstOrDefault(o => o.IsSystemDefault && o.IsConnected)?.DeviceID);

            if (preferredDeviceID is null)
            {
                UniLog.Warning("Cannot find a preferred device ID");
                return false;
            }

            var preferredStreamingDeviceID = preferredStreamingDevice?.EntryID.Value;

            if (preferredDeviceID == __instance.DefaultAudioOutput?.DeviceID && preferredStreamingDeviceID == __instance.StreamingAudioOutput?.DeviceID)
                return false;

            __instance.PrimaryOutput.Device = null;
            __instance.StreamingOutput.Device = null;

            var oldDefault = __instance.DefaultAudioOutput;
            var oldStreaming = __instance.StreamingAudioOutput;

            if (oldDefault is not null)
            {
                oldDefault.Stop();
                oldDefault.RenderAudio = null;
            }

            if (oldStreaming is not null)
            {
                oldStreaming.Stop();
                oldStreaming.RenderAudio = null;
            }

            __instance._defaultAudioOutputIndex = __instance.AudioOutputs.FindIndex(i => i.DeviceID == preferredDeviceID);
            __instance._streamingAudioOutputIndex = __instance.AudioOutputs.FindIndex(i => i.DeviceID == preferredStreamingDeviceID);

            if (__instance._streamingAudioOutputIndex >= 0 && __instance._streamingAudioOutputIndex == __instance._defaultAudioOutputIndex)
                __instance._streamingAudioOutputIndex = -1;

            var newDefault = __instance.DefaultAudioOutput!;
            var newStreaming = __instance.StreamingAudioOutput;

            __instance.audioThreadLoop.Supress = true;

            newDefault.RenderAudio = __instance.RenderAudio;
            newDefault.Start("Default Output");

            if (newStreaming is not null)
            {
                newStreaming.RenderAudio = __instance.RenderAudio;
                newStreaming.Start("Streaming Camera");
            }

            __instance.PrimaryOutput.Device = newDefault;
            __instance.StreamingOutput.Device = newStreaming;

            ___DefaultAudioOutputChanged?.Invoke(newDefault);
            return false;
        }
    }
}