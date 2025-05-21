using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Locale;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable MHA008 // Assignment to non-ref patch method argument

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(LocalizedByteFormatting))]
    internal sealed class LocalizedByteFormatting : ResoniteAsyncEventHandlerMonkey<LocalizedByteFormatting, LocaleLoadingEvent>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.Last;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnitFormatting), nameof(UnitFormatting.FormatBytes))]
        private static bool UnitFormatBytesPrefix(double bytes, int decimalPlaces, ref string __result)
        {
            if (!Enabled)
                return true;

            var format = $"F{decimalPlaces}";
            var culture = Settings.GetActiveSetting<LocaleSettings>()?.ActiveCulture ?? CultureInfo.CurrentCulture;

            // Basically just `2^(10*n)`, but also limited to max unit index.
            uint index = MathX.Min(MathX.FloorToUInt(MathX.Log(MathX.Abs(bytes), 2) / 10), (uint)(UnitFormatting.suffixes.Length -1));
            string suffix = UnitFormatting.suffixes[index];
            // AKA scaled bytes in IEC format 
            var numToFormat = bytes / MathX.Pow(1024, index);
            __result = $"{numToFormat.ToString(format, culture)} {Mod.GetMessageInCurrent($"StorageUnits.{suffix}")}";
            return false;
        }

        private static readonly ConditionalWeakTable<StorageUsageStatus, CultureInfo> _lastCultureByStorageStatus = new();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageUsageStatus), nameof(StorageUsageStatus.OnCommonUpdate))]
        private static bool StorageUsageStatusOnCommonUpdatePrefix(StorageUsageStatus __instance)
        {
            if (!Enabled)
                return true;

            if (__instance.World != Userspace.UserspaceWorld && __instance.World != Userspace.CloudHome)
                return false;

            var storage = (__instance.OwnerId.Value is null || __instance.OwnerId.Value == __instance.Engine.Cloud.Session.CurrentUserID)
                ? __instance.Engine.Cloud.Storage.CurrentStorage
                : ((!__instance.GroupMemberQuota.Value)
                    ? __instance.Engine.Cloud.Groups.TryGetGroupStorage(__instance.OwnerId.Value)
                    : __instance.Engine.Cloud.Groups.TryGetMemberStorage(__instance.OwnerId.Value));

            if (storage is null || storage.QuotaBytes <= 0)
            {
                __instance.HasValidData.Value = false;

                __instance.StorageBytes.Value = -1L;
                __instance.FullStorageBytes.Value = -1L;
                __instance.ShareableStorageBytes.Value = -1L;
                __instance.SharedStorageBytes.Value = -1L;
                __instance.UsageBytes.Value = -1L;
                __instance.UsageRatio.Value = -1f;

                __instance.StorageString.Value = "---";
                __instance.FullStorageString.Value = "---";
                __instance.ShareableStorageString.Value = "---";
                __instance.SharedStorageString.Value = "---";
                __instance.RatioString.Value = "---";
                __instance.UsageString.Value = "---";

                return false;
            }

            var activeCulture = Settings.GetActiveSetting<LocaleSettings>()?.ActiveCulture ?? CultureInfo.CurrentCulture;
            var cultureChanged = !_lastCultureByStorageStatus.TryGetValue(__instance, out _);

            __instance.HasValidData.Value = true;

            var quotaBytes = storage.QuotaBytes;
            var usedBytes = storage.UsedBytes;

            __instance.StorageBytes.Value = quotaBytes;
            __instance.FullStorageBytes.Value = storage.FullQuotaBytes;
            __instance.ShareableStorageBytes.Value = storage.ShareableQuotaBytes;
            __instance.SharedStorageBytes.Value = storage.SharedQuotaBytes;
            __instance.UsageBytes.Value = usedBytes;
            __instance.UsageRatio.Value = (float)usedBytes / quotaBytes;

            if (__instance.StorageBytes.GetWasChangedAndClear() || cultureChanged)
                __instance.StorageString.Value = UnitFormatting.FormatBytes(quotaBytes);

            if (__instance.FullStorageBytes.GetWasChangedAndClear() || cultureChanged)
                __instance.FullStorageString.Value = UnitFormatting.FormatBytes(__instance.FullStorageBytes);

            if (__instance.ShareableStorageBytes.GetWasChangedAndClear() || cultureChanged)
                __instance.ShareableStorageString.Value = UnitFormatting.FormatBytes(__instance.ShareableStorageBytes);

            if (__instance.SharedStorageBytes.GetWasChangedAndClear() || cultureChanged)
                __instance.SharedStorageString.Value = UnitFormatting.FormatBytes(__instance.SharedStorageBytes);

            if (__instance.UsageBytes.GetWasChangedAndClear() || cultureChanged)
                __instance.UsageString.Value = UnitFormatting.FormatBytes(usedBytes);

            if (__instance.UsageRatio.GetWasChangedAndClear() || cultureChanged)
                __instance.RatioString.Value = $"{MathX.RoundToInt(__instance.UsageRatio.Value * 100f).ToString(activeCulture)} %";

            _lastCultureByStorageStatus.Remove(__instance);
            _lastCultureByStorageStatus.Add(__instance, activeCulture);

            return false;
        }

        protected override Task Handle(LocaleLoadingEvent eventData)
        {
            Engine.Current.GlobalCoroutineManager.RunInSeconds(2, _lastCultureByStorageStatus.Clear);

            return Task.CompletedTask;
        }
    }
}

#pragma warning restore MHA008 // Assignment to non-ref patch method argument
