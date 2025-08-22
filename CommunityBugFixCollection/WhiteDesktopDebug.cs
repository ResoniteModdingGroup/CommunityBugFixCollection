using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static FrooxEngine.LocomotionSimulator;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(WhiteDesktopDebug))]
    [HarmonyPatch(typeof(LocomotionSimulator), nameof(LocomotionSimulator.SimulateHead))]
    internal sealed class WhiteDesktopDebug : ResoniteBugFixMonkey<WhiteDesktopDebug>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        private static bool Prefix(LocomotionSimulator __instance, in MoveData data)
        {
            if (__instance.SimulateHeadLook)
            {
                __instance._headLookAdjustmentAge += data.deltaTime;
                __instance.HeadLookDirection = data.invRotationDelta * AsRefReadOnly(__instance.HeadLookDirection);
                if (__instance._headLookAdjustmentAge > 2f || MathX.Angle(__instance.HeadLookDirection, float3.Forward) > __instance.HeadMaxLookDirectionAngle)
                {
                    __instance._headLookAdjustmentAge = 0f;
                    __instance.HeadLookDirection = floatQ.AxisAngle(float3.Right, RandomX.Range(-15f, 15f)) * floatQ.AxisAngle(float3.Up, RandomX.Range(-30f, 30f)) * float3.Forward;
                }
            }

            __instance._currentHeadLookDirection = data.invRotationDelta * __instance._currentHeadLookDirection;
            __instance._currentHeadLookDirection = MathX.SmoothSlerp(in __instance._currentHeadLookDirection, __instance.HeadLookDirection, ref __instance._intermediateHeadLookDirection, data.deltaTime * __instance.HeadLookSpeed);
            __instance._currentHeadLookDirection = MathX.LimitSwing(__instance._currentHeadLookDirection, float3.Forward, __instance.HeadMaxLookDirectionAngle);
            __instance._headSwaySpeedProgress += data.deltaTime * __instance.HeadSwaySpeedSpeed;
            float headSwaySpeed = MathX.LerpUnclamped(lerp: MathX.SimplexNoise(__instance._headSwaySpeedProgress + __instance._headSwaySpeedSeed).Pack(), a: __instance.HeadSwayMinSpeed, b: __instance.HeadSwayMaxSpeed);
            __instance._headSwayProgress += data.deltaTime * headSwaySpeed;
            float verticalSwayAngle = MathX.SimplexNoise(__instance._headSwayVerticalSeed + __instance._headSwayProgress);
            float horizontalSwayAngle = MathX.SimplexNoise(__instance._headSwayHorizontalSeed + __instance._headSwayProgress);
            verticalSwayAngle *= __instance.HeadSwayVerticalAngle;
            horizontalSwayAngle *= __instance.HeadSwayHorizontalAngle;
            float horizontalTilt = 0f;
            float verticalTilt = 0f;
            float verticalOffset = 0f;
            float3 referenceDir = MathX.Slerp(float3.Forward, in data.positionDir, __instance.MinVelocityLerp);

            if (float.IsNaN(referenceDir.z))
                Logger.Error(() => "referenceDir.z is NaN!");

            int forwardMul = MathX.Sign(referenceDir.z);
            if ((float)forwardMul == 0f)
            {
                forwardMul = 1;
            }
            if (MathX.Approximately(in referenceDir, float3.Zero))
            {
                referenceDir = float3.Forward;
            }
            floatQ rotationAlign = floatQ.FromToRotation(in referenceDir, float3.Forward);
            if (__instance.State == LocomotionState.OnGround)
            {
                foreach (LocomotionFoot foot in __instance._feet)
                {
                    float3 @float = rotationAlign * AsRefReadOnly(foot.BasePosition);
                    float footDistanceLerp = MathX.Pow(1f - MathX.Clamp01(foot.BasePosition.Magnitude / __instance.FootMaxDistance), 0.25f);
                    _ = foot.Position;

                    if (float.IsNaN(@float.x))
                        Logger.Error(() => "@float.x is NaN!");

                    int horizontalSide = MathX.Sign(@float.x);
                    float forwardOffset = MathX.Clamp((@float.z - (rotationAlign * AsRefReadOnly(foot.RestingPosition)).z) / (__instance.DirectionFootTravelDistance * 0.5f), -1f, 1f);
                    horizontalTilt -= forwardOffset * (float)horizontalSide * (float)forwardMul;
                    verticalOffset -= MathX.Abs(forwardOffset);
                }
                if (__instance._feet.Count > 0)
                {
                    verticalOffset /= (float)__instance._feet.Count;
                }
            }
            else
            {
                if (float.IsNaN(__instance.CurrentPositionVelocity.z))
                    Logger.Error(() => "__instance.CurrentPositionVelocity.z is NaN!");

                verticalTilt = (0f - MathX.Clamp(__instance.CurrentPositionVelocity.y / __instance.MaxVerticalReferenceSpeed, -1f, 1f)) * __instance.HeadJumpAngle * (float)MathX.Sign(__instance.CurrentPositionVelocity.z);
            }
            __instance._headVerticalPosition -= data.positionDelta.y * ((__instance.State == LocomotionState.Floating) ? __instance.HeadFloatingVerticalTransferRatio : __instance.HeadVerticalTransferRatio);
            float drag;
            float force;
            if (__instance.State == LocomotionState.Floating)
            {
                drag = __instance.HeadFloatingVerticalDrag;
                force = __instance.HeadFloatingVerticalForce;
            }
            else
            {
                drag = __instance.HeadVerticalDrag;
                force = __instance.HeadVerticalForce;
            }

            if (float.IsNaN(__instance._headVerticalVelocity))
                Logger.Error(() => "__instance._headVerticalVelocity is NaN!");

            int headVerticalDir = MathX.Sign(__instance._headVerticalVelocity);
            __instance._headVerticalVelocity = MathX.Abs(__instance._headVerticalVelocity);
            __instance._headVerticalVelocity -= data.deltaTime * drag * __instance._headVerticalVelocity;
            __instance._headVerticalVelocity = MathX.Max(0f, __instance._headVerticalVelocity);
            __instance._headVerticalVelocity = MathX.Min(__instance.HeadMaxVerticalVelocity, __instance._headVerticalVelocity);
            __instance._headVerticalVelocity *= headVerticalDir;
            __instance._headVerticalVelocity += (0f - __instance._headVerticalPosition) * data.deltaTime * force;
            __instance._headVerticalPosition += __instance._headVerticalVelocity * data.deltaTime;
            __instance._headVerticalPosition = MathX.Clamp(__instance._headVerticalPosition, __instance.HeadMinVerticalOffset, __instance.HeadMaxVerticalOffset);
            float multiplier = MathX.Clamp01(__instance.MinVelocityLerp + __instance.AirLerp + __instance.FloatingLerp);
            horizontalTilt *= multiplier;
            verticalTilt *= multiplier;
            verticalOffset *= multiplier;
            __instance._currentHorizontalTilt = MathX.SmoothDamp(__instance._currentHorizontalTilt, horizontalTilt, ref __instance._intermediateHorizontalTilt, __instance.CurrentParameters.HeadSmoothingSpeed, 10f, data.deltaTime);
            __instance._currentVerticalTilt = MathX.SmoothDamp(__instance._currentVerticalTilt, verticalTilt, ref __instance._intermediateVerticalTilt, __instance.CurrentParameters.HeadSmoothingSpeed, 10f, data.deltaTime);
            __instance._currentVerticalOffset = MathX.SmoothDamp(__instance._currentVerticalOffset, verticalOffset, ref __instance._intermediateVerticalOffset, __instance.CurrentParameters.HeadSmoothingSpeed, 10f, data.deltaTime);
            __instance.HeadPositionOffset = new float3(__instance._currentHorizontalTilt * __instance.CurrentParameters.HeadHorizontalBobOffset, __instance._currentVerticalOffset * __instance.CurrentParameters.HeadVerticalBobOffset + __instance._headVerticalPosition);
            __instance.HeadRotationOffset = floatQ.AxisAngle(float3.Right, __instance._currentVerticalTilt + verticalSwayAngle + __instance.CurrentParameters.VerticalHeadAngleOffset) * floatQ.AxisAngle(float3.Forward, __instance._currentHorizontalTilt * __instance.CurrentParameters.HeadHorizontalBobAngle + horizontalSwayAngle) * floatQ.LookRotation(in __instance._currentHeadLookDirection);

            static ref readonly T AsRefReadOnly<T>(in T temp)
            {
                //ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
                return ref temp;
            }

            return false;
        }
    }
}