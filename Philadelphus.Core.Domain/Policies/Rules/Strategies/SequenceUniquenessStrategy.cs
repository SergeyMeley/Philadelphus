using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Фабрика стратегий проверки уникальности свойства Sequence.
    /// </summary>
    internal static class SequenceUniquenessStrategy
    {
        public static ISequenceUniquenessStrategy<TreeRootModel> TreeRoot()
        {
            return new SequenceUniquenessStrategy<TreeRootModel>(
                model => model.OwningWorkingTree.ContentRoot != null
                    ? new[] { ToSequencedItem(model.OwningWorkingTree.ContentRoot) }
                    : Enumerable.Empty<SequencedItem>());
        }

        public static ISequenceUniquenessStrategy<TreeNodeModel> TreeNode()
        {
            return new SequenceUniquenessStrategy<TreeNodeModel>(
                model => model.Parent switch
                {
                    TreeRootModel root => root.ChildNodes.Select(ToSequencedItem),
                    TreeNodeModel node => node.ChildNodes.Select(ToSequencedItem),
                    _ => Enumerable.Empty<SequencedItem>(),
                });
        }

        public static ISequenceUniquenessStrategy<TreeLeaveModel> TreeLeave()
        {
            return new SequenceUniquenessStrategy<TreeLeaveModel>(
                model => model.ParentNode?.ChildLeaves.Select(ToSequencedItem) ?? Enumerable.Empty<SequencedItem>());
        }

        public static ISequenceUniquenessStrategy<ElementAttributeModel> ElementAttribute()
        {
            return new SequenceUniquenessStrategy<ElementAttributeModel>(
                model => PolicyRuleModelQueries.GetDirectAttributes(model.Owner).Select(ToSequencedItem));
        }

        private static SequencedItem ToSequencedItem(IShrubMemberModel model)
        {
            return new SequencedItem(model.Uuid, model.Sequence);
        }
    }
}
