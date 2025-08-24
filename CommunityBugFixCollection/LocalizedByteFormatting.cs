using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Configuration;
using MonkeyLoader.Meta;
using MonkeyLoader.Patching;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.Locale;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommunityBugFixCollection
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(LocalizedByteFormatting))]
    internal sealed class LocalizedByteFormatting : ResoniteAsyncEventHandlerMonkey<LocalizedByteFormatting, LocaleLoadingEvent>, IConfiguredMonkey<BugFixOptions>
    {
        private static readonly ConditionalWeakTable<StorageUsageStatus, CultureInfo> _lastCultureByStorageStatus = [];

        public override IEnumerable<string> Authors { get; } = [.. Contributors.Banane9, .. Contributors.LJ];

        public override bool CanBeDisabled => true;

        BugFixOptions IConfiguredMonkey<BugFixOptions>.ConfigSection => ConfigSection;
        ConfigSection IConfiguredMonkey.ConfigSection => ConfigSection;

        public override int Priority => HarmonyLib.Priority.Last;

        /// <inheritdoc cref="ResoniteBugFixMonkey{TMonkey}.ConfigSection"/>
        private static BugFixOptions ConfigSection => BugFixOptions.Instance;

        protected override Task Handle(LocaleLoadingEvent eventData)
        {
            Engine.Current.GlobalCoroutineManager.RunInSeconds(2, _lastCultureByStorageStatus.Clear);

            return Task.CompletedTask;
        }

        protected override bool OnEngineReady()
        {
            ConfigSection.ItemChanged += ConfigSectionItemChanged;

            return base.OnEngineReady();
        }

        /// <inheritdoc cref="ConfiguredMonkey{TMonkey, TConfigSection}.OnLoaded"/>
        protected override bool OnLoaded()
        {
            if (BugFixOptions.Instance is null)
                Config.LoadSection<BugFixOptions>();

            return base.OnLoaded();
        }

        protected override bool OnShutdown(bool applicationExiting)
        {
            if (!applicationExiting)
                ConfigSection.ItemChanged -= ConfigSectionItemChanged;

            return base.OnShutdown(applicationExiting);
        }

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnitFormatting), nameof(UnitFormatting.FormatBytes))]
        private static bool UnitFormatBytesPrefix(double bytes, int decimalPlaces, ref string __result)
        {
            if (!Enabled)
                return true;

            var format = $"F{decimalPlaces}";
            var culture = Settings.GetActiveSetting<LocaleSettings>()?.ActiveCulture ?? CultureInfo.CurrentCulture;

            // Select base and divisor for suffix determination
            var baseNum = ConfigSection.UseIecByteFormat ? 2 : 10;
            var divNum = ConfigSection.UseIecByteFormat ? 10 : 3;

            // Either `2^(10*n)` or `10^(3*n)`, but also limited to max unit index.
            var index = MathX.Min(MathX.FloorToUInt(MathX.Log(MathX.Abs(bytes), baseNum) / divNum), (uint)(UnitFormatting.suffixes.Length - 1));
            var suffix = UnitFormatting.suffixes[index];

            if (ConfigSection.UseIecByteFormat)
                suffix = suffix.Insert(suffix.Length - 1, "i");

            // AKA scaled bytes in IEC/decimal format
            var numToFormat = bytes / MathX.Pow(baseNum, divNum * index);
            __result = $"{numToFormat.ToString(format, culture)} {Mod.GetMessageInCurrent($"StorageUnits.{suffix}")}";

            return false;
        }

        private void ConfigSectionItemChanged(object sender, IConfigKeyChangedEventArgs configKeyChangedEventArgs)
            => Engine.Current.GlobalCoroutineManager.RunInSeconds(0, _lastCultureByStorageStatus.Clear);
    }
}