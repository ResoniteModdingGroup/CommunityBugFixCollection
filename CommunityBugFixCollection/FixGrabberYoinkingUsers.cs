using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using Elements.Core;
using MonkeyLoader.Logging;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(FixGrabberYoinkingUsers))]
    [HarmonyPatch(typeof(Grabber))]

    internal sealed class FixGrabberYoinkingUsers : ResoniteBugFixMonkey<FixGrabberYoinkingUsers>
    {
        public override IEnumerable<string> Authors => Contributors.Onan;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Instructions)
        {
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            Logger.Debug(() => $"Patching IL instructions!");
            List<CodeInstruction> Instructions2 = new(Instructions);


            object loadfield = Instructions2.Find((o) =>
            {
                return o.opcode == OpCodes.Ldfld && o.operand.ToString().Contains("_grabbedObjects");
            }).operand;

            int injectLocation = Instructions2.FindIndex((o) =>
            {
                return o.opcode == OpCodes.Call && o.operand.ToString().Contains("EndUndoBatch");
            });

            Instructions2.InsertRange(injectLocation + 1, new CodeInstruction[]{
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldfld, loadfield),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.DeclaredMethod(typeof(FixGrabberYoinkingUsers), nameof(CorrectGrabberPositionLaser)))}
                );


            return Instructions2;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");
            Logger.Debug(() => $"Finding valid methods!");

            var methodNames = new[]
            {
                    AccessTools.DeclaredMethod(typeof(Grabber), nameof(Grabber.Grab), new Type[]{ typeof(List<IGrabbable>), typeof(bool)})
                };

            return methodNames;
        }

        public static void CorrectGrabberPositionLaser(Grabber __instance, List<IGrabbable> _grabbedObjects)
        {
            if (!Enabled)
                return;

            float3 position = float3.Zero;

            float3[] positions_original = new float3[_grabbedObjects.Count];

            for (int i = 0; i < positions_original.Length; i++)
            {
                position += _grabbedObjects[i].Slot.GlobalPosition;
                positions_original[i] = _grabbedObjects[i].Slot.GlobalPosition;
            }


            position /= _grabbedObjects.Count;



            __instance.HolderSlot.GlobalPosition = position;

            for (int i = 0; i < positions_original.Length; i++)
            {
                _grabbedObjects[i].Slot.GlobalPosition = positions_original[i];
            }


            Logger.Debug(() => $"Correcting grab position!");
        }
    }


}