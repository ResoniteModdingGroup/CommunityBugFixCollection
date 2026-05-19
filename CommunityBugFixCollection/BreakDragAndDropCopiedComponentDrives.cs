using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(BreakDragAndDropCopiedComponentDrives))]
    internal sealed class BreakDragAndDropCopiedComponentDrives : ResoniteBugFixMonkey<BreakDragAndDropCopiedComponentDrives>
    {
        public override IEnumerable<string> Authors { get; } = [.. Contributors.Banane9, .. Contributors.Onan];

        public static MethodBase TargetMethod()
        {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Type stateMachineType = typeof(SlotComponentReceiver).InnerTypes().FirstOrDefault(x => x.Name.StartsWith("<>") && x.InnerTypes().Any());
            MethodInfo stolencodethingy = AccessTools.GetDeclaredMethods(stateMachineType).FirstOrDefault(x => x.Name == "<TryReceive>b__1");
            return stolencodethingy;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }


        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> Codes)
        {
            List<CodeInstruction> newcodes = new List<CodeInstruction>();
            foreach(CodeInstruction code in Codes)
            {
                if (code.Calls(AccessTools.Method(typeof(ContainerWorker<>), "CopyComponent")))
                {

                    //newcodes.Add(new CodeInstruction(OpCodes.ld));
                    newcodes.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(FrooxEngine.Slot), "DuplicateComponent")));
                    newcodes.Add(new CodeInstruction(OpCodes.Pop));
                }
                else
                {
                    newcodes.Add(code);
                }
            }
            Logger.Info(newcodes);
            return newcodes;
        }
    }
}