using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.Globalization;

namespace CommunityBugFixCollection
{
    [HarmonyPatch(typeof(ListEditor))]
    [HarmonyPatchCategory(nameof(BetterListEditor))]
    internal sealed class BetterListEditor : ResoniteBugFixMonkey<BetterListEditor>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ListEditor.Target_ElementsAdded))]
        private static bool ElementsAddedPrefix(ListEditor __instance, ISyncList list, int startIndex, int count)
        {
            if (!Enabled)
                return true;

            if (count is 0)
                return false;

            var addedElements = Pool.BorrowList<ISyncMember>();

            for (var i = startIndex; i < startIndex + count; ++i)
                addedElements.Add(list.GetElement(i));

            __instance.World.RunSynchronously(() =>
            {
                for (var i = __instance.Slot.ChildrenCount - 1; i >= startIndex; --i)
                    __instance.Slot[i].OrderOffset += count;

                for (var i = 0; i < addedElements.Count; ++i)
                {
                    if (addedElements[i].FilterWorldElement() is null)
                        continue;

                    var slot = __instance.Slot.AddSlot("Element");
                    slot.OrderOffset = startIndex + i;

                    __instance.BuildListItem(list, startIndex + i, addedElements[i], slot);
                }

                Pool.Return(ref addedElements);
            });

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ListEditor.GetElementName))]
        private static bool GetElementNamePrefix(int index, ref string __result)
        {
            if (!Enabled)
                return true;

            __result = index.ToString(CultureInfo.InvariantCulture) + ':';
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ListEditor.MoveElement))]
        private static bool MoveElementPrefix(ListEditor __instance, IButton button, int offset)
        {
            if (!Enabled)
                return true;

            if (__instance._targetList.Target.FilterWorldElement() is null)
                return false;

            if (__instance._targetList.Target is ConflictingSyncElement { DirectAccessOnly: true } && !__instance.LocalUser.IsDirectlyInteracting())
                return false;

            var index = __instance.FindButtonIndex(button);
            var newIndex = index + offset;

            if (index < 0 || newIndex < 0 || newIndex >= __instance._targetList.Target.Count)
                return false;

            __instance._targetList.Target.MoveElementToIndex(index, newIndex);

            __instance.reindex = true;
            __instance.MarkChangeDirty();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ListEditor.Reindex))]
        private static bool ReindexPrefix(ListEditor __instance)
        {
            if (!Enabled)
                return true;

            for (var i = 0; i < __instance.Slot.ChildrenCount; ++i)
            {
                if (__instance.Slot[i].GetComponentInChildren<Text>() is not Text text)
                    continue;

                text.Content.Value = __instance.GetElementName(__instance._targetList.Target, i);
            }

            return false;
        }
    }
}