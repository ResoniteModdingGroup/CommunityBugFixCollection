using Elements.Core;
using FrooxEngine;
using MonkeyLoader.Resonite;
using MonkeyLoader.Resonite.UI.ContextMenus;

namespace CommunityBugFixCollection
{
    internal sealed class BreakDragAndDropCopiedComponentDrives
        : ResoniteAsyncEventHandlerMonkey<BreakDragAndDropCopiedComponentDrives, ContextMenuItemsGenerationEvent<SlotComponentReceiver>>
    {
        private const string CopyComponentLocaleKey = "Inspector.Actions.CopyComponent";

        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        public override int Priority => HarmonyLib.Priority.First;

        protected override Task Handle(ContextMenuItemsGenerationEvent<SlotComponentReceiver> eventData)
        {
            var receiver = eventData.Summoner;
            var receiverTarget = receiver.Target.Target;

            if (receiverTarget is null)
                return Task.CompletedTask;

            var itemsRoot = eventData.ContextMenu._itemsRoot.Target;
            var componentReference = eventData.LastDroppedGrabbables.UntypedReferences
                .OfType<Component>()
                .FirstOrDefault(component => component.Slot != receiver.Target.Target);

            if (componentReference is null)
                return Task.CompletedTask;

            // Add explicit order offset to be able to replace the vanilla entry and keep its position
            // Don't have to worry about pagination, as that's only ever added *after* event handlers
            for (var i = 0; i < itemsRoot.ChildrenCount; ++i)
                itemsRoot[i].OrderOffset = -10 * (itemsRoot.ChildrenCount - i);

            var copyItemSlot = itemsRoot.Children
                .FirstOrDefault(static itemSlot => itemSlot
                    .GetComponentInChildren<LocaleStringDriver>(static driver => driver.Key.Value == CopyComponentLocaleKey) is not null);

            if (copyItemSlot is null)
                return Task.CompletedTask;

            var copyItemOffset = copyItemSlot.OrderOffset;
            copyItemSlot.Destroy();

            var copyItem = eventData.ContextMenu.AddItem(CopyComponentLocaleKey.AsLocaleKey(), (Uri)null!, RadiantUI_Constants.Hero.GREEN);
            copyItem.Slot.OrderOffset = copyItemOffset;

            copyItem.Button.LocalPressed += delegate
            {
                receiver.Target.Target.DuplicateComponent(componentReference);
                receiver.LocalUser.CloseContextMenu(receiver);
            };

            return Task.CompletedTask;
        }
    }
}