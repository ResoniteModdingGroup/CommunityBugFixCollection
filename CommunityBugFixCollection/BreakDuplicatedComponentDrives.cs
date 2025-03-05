using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Text;

using static FrooxEngine.Worker;

namespace CommunityBugFixCollection
{
    [HarmonyPatchCategory(nameof(BreakDuplicatedComponentDrives))]
    [HarmonyPatch(typeof(Slot), nameof(Slot.DuplicateComponents), [typeof(List<Component>), typeof(bool), typeof(List<Component>)])]
    internal sealed class BreakDuplicatedComponentDrives : ResoniteMonkey<BreakDuplicatedComponentDrives>
    {
        public override IEnumerable<string> Authors => Contributors.Banane9;

        public override bool CanBeDisabled => true;

        private static void CollectInternalReferences(List<Component> sourceComponents, InternalReferences internalRefs, HashSet<ISyncRef> externalRefs, HashSet<ISyncRef> breakRefs)
        {
            foreach (var component in sourceComponents)
            {
                var refList = Pool.BorrowList<ISyncRef>();
                component.GetSyncMembers(refList, true);

                foreach (var syncRef in refList)
                {
                    if (syncRef.Target is null)
                    {
                        if (syncRef.Value != RefID.Null)
                            breakRefs.Add(syncRef);

                        continue;
                    }

                    var targetParent = syncRef.Target?.FindNearestParent<Component>();

                    // Parent can't be a component being duplicated currently
                    if (targetParent is null)
                        externalRefs.Add(syncRef);

                    // A HashSet for the Contains would seem faster, but in 99.9% of cases this is only one component
                    if (sourceComponents.Contains(targetParent!))
                    {
                        internalRefs.AddPair(syncRef, syncRef.Target!);
                        continue;
                    }

                    externalRefs.Add(syncRef);

                    if (syncRef is ILinkRef)
                        breakRefs.Add(syncRef);
                }

                Pool.Return(ref refList);
            }
        }

        private static bool Prefix(Slot __instance, List<Component> sourceComponents, bool breakExternalReferences, List<Component> duplicates)
        {
            if (!Enabled)
                return true;

            using var internalRefs = new InternalReferences();
            var breakRefs = Pool.BorrowHashSet<ISyncRef>();
            var externalRefs = Pool.BorrowHashSet<ISyncRef>();

            CollectInternalReferences(sourceComponents, internalRefs, externalRefs, breakRefs);

            if (!breakExternalReferences)
                externalRefs.Clear();

            breakRefs.UnionWith(externalRefs);

            foreach (var sourceComponent in sourceComponents)
            {
                var duplicatedComponent = __instance.AttachComponent(sourceComponent.GetType(), runOnAttachBehavior: false);

                internalRefs.RegisterCopy(sourceComponent, duplicatedComponent);
                duplicatedComponent.CopyValues(sourceComponent, (from, to) => MemberCopy(from, to, internalRefs, breakRefs, checkTypes: false));
                duplicates.Add(duplicatedComponent);
            }

            internalRefs.TransferReferences(true);

            foreach (var duplicate in duplicates)
                duplicate.RunDuplicate();

            Pool.Return(ref breakRefs);
            Pool.Return(ref externalRefs);

            return false;
        }
    }
}