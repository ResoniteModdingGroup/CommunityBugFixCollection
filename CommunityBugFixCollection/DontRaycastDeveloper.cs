using FrooxEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(DontRaycastDeveloper))]
    [HarmonyPatch(typeof(FrooxEngine.RaycastDriver), nameof(FrooxEngine.RaycastDriver.OnCommonUpdate))]
    internal sealed class DontRaycastDeveloper : ResoniteBugFixMonkey<DontRaycastDeveloper>
    {

        public override IEnumerable<string> Authors { get; } = [.. Contributors.Onan];
        public static FieldInfo IgnoreHierarchy = AccessTools.Field(typeof(RaycastDriver), "IgnoreHierarchy");//get ignore hierarchy field.
        public static FieldInfo Filter = AccessTools.Field(typeof(RaycastDriver), "Filter");//get Filter func field.
        public static MethodInfo Func2_Call = AccessTools.Method(typeof(Func<>), "Invoke", new Type[] { typeof(FrooxEngine.ICollider)}, new Type[] { typeof(FrooxEngine.ICollider), typeof(bool) }); //tries to get a call to a function (like Filter) of name "Invoke" with the generic type ("ICollider", "bool") and given a value of "ICollider" in the call from the stack.


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Debugger.Launch();
            List<CodeInstruction> instructionList = [.. instructions];


            int index = 0;
            int start = 0;
            int stop = 0;
            foreach (CodeInstruction inst in instructionList)
            {
                if (inst.opcode == OpCodes.Ldfld && inst.operand?.Equals(IgnoreHierarchy) == true)
                {
                    if (start == 0)
                    {
                        start = index;
                    }
                }
                if (inst.operand?.Equals(Func2_Call) == true)
                {
                    if (stop == 0)
                    {
                        stop = index;
                        break;
                    }
                }
                index++;
            }


            //note to self, loading "this" before trying to load a "this" variable on the stack is important - @989onan
            List<CodeInstruction> newinstructions =
            [
                new CodeInstruction(OpCodes.Ldarg_0),
                instructionList[start],
                new CodeInstruction(OpCodes.Ldloc_S,8),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Filter),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DontRaycastDeveloper), "DontRaycastDeveloperFilter")),
            ];
            instructionList.RemoveRange(start-1, (stop+2) - start);
            


            instructionList.InsertRange(start - 1/*this is needed since start lands on "ignore hiearchy" which is at the beginning*/, newinstructions);
            

            return instructionList;

        }


        //recreates the if statement, allowing for full control over this check on raycasts and doing whatever we want.
        public static bool DontRaycastDeveloperFilter(SyncRef<Slot> IgnoreHierarchy, RaycastHit hit, SyncDelegate<Func<ICollider, bool>> Filter) {
            if (!Enabled)
            {
                return (IgnoreHierarchy.Target == null || !hit.Collider.Slot.IsChildOf(IgnoreHierarchy.Target, true)) && (Filter.Target == null || Filter.Target(hit.Collider)); //this is where we use the default behavior.
            }
            return (IgnoreHierarchy.Target == null || !hit.Collider.Slot.IsChildOf(IgnoreHierarchy.Target, true)) && (Filter.Target == null || Filter.Target(hit.Collider)) && hit.Collider.Slot.FindParent(o=>o.Name == "Gizmo")?.GetComponent<SlotGizmo>() == null; //this is where we can specify "STOP RAYCASTING DEVELOPER!!!"
        }
    }
}
