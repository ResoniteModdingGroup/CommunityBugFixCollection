using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader;
using MonkeyLoader.Components;
using MonkeyLoader.Configuration;
using MonkeyLoader.Resonite;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(TiltedUIAlignment))]
    [HarmonyPatch(typeof(UI_TargettingController), nameof(UI_TargettingController.OnBeforeHeadUpdate))]
    internal sealed class TiltedUIAlignment : ResoniteMonkey<TiltedUIAlignment>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        protected override bool OnEngineReady()
        {
            var wasSetFromDefault = false;

            void SetFromDefaultHandler(object sender, ConfigKeyChangedEventArgs<bool> eventArgs)
                => wasSetFromDefault = eventArgs.Label == ConfigKey.SetFromDefaultEventLabel;

            EnabledToggle!.Changed += SetFromDefaultHandler;
            EnabledToggle.GetValue();
            EnabledToggle.Changed -= SetFromDefaultHandler;

            if (wasSetFromDefault)
            {
                Logger.Info(() => "Enabled was set from default, applying GPU detection!");

                var unitySystemInfoType = AccessTools.TypeByName("UnityEngine.SystemInfo, UnityEngine.CoreModule");
                Logger.Info(() => $"Unity SystemInfo type is: {(unitySystemInfoType is null ? "null" : unitySystemInfoType.CompactDescription())}");

                var getGraphicsDeviceName = unitySystemInfoType is null ? null : AccessTools.DeclaredPropertyGetter(unitySystemInfoType, "graphicsDeviceName");

                if (getGraphicsDeviceName is null)
                {
                    Logger.Warn(() => "Did not find UnityEngine.SystemInfo to check GPU name - disabling tilt!");

                    EnabledToggle!.SetValue(false, "GPU-Detection.Fail");
                }
                else
                {
                    Logger.Debug(() => "Using UnityEngine.SystemInfo to check GPU name!");

                    var gpu = (string?)getGraphicsDeviceName?.Invoke(null, null);
                    var isAmd = gpu?.Contains("AMD", StringComparison.OrdinalIgnoreCase) ?? false;
                    var isIntel = gpu?.Contains("Intel", StringComparison.OrdinalIgnoreCase) ?? false;
                    var enableTilt = isAmd || isIntel;

                    Logger.Info(() => $"Detected GPU [{gpu}] - {(enableTilt ? "enabled" : "disabled")} tilt!");

                    EnabledToggle!.SetValue(enableTilt, "GPU-Detection.Success");
                }
            }
            else
            {
                Logger.Info(() => "Enabled wasn't set from default, not applying GPU detection!");
            }

            return base.OnEngineReady();
        }

        private static void Postfix(UI_TargettingController __instance)
        {
            if (!Enabled)
                return;

            var space = __instance.ViewSpace ?? __instance.Slot;
            var spacePosition = space.GlobalPosition;
            var rootDistance = MathX.Max(1, MathX.MaxComponent(MathX.Abs(spacePosition)));

            var rotationAxis = float3.Right;
            var angle = MathX.Clamp(0.1f, 5, 0.01f * MathX.Sqrt(rootDistance));

            // Add angle to camera to prevent flickering
            var rotation = floatQ.AxisAngle(rotationAxis, angle);
            __instance.ViewRotation *= rotation;

            //  Adjust position based on angle to frame UI properly still
            var antiRotation = floatQ.AxisAngle(rotationAxis, angle + 2);
            __instance.ViewPosition = __instance._currentCenter + (antiRotation * __instance._currentPlane * __instance._currentDistance);
        }
    }
}