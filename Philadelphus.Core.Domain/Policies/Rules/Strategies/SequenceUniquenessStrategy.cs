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
            // У рабочего дерева может быть только один корень, но правило оставлено общим,
            // чтобы Sequence корня проходил ту же проверку положительности и уникальности, что и остальные элементы.
            return new SequenceUniquenessStrategy<TreeRootModel>(
                model => model.OwningWorkingTree.ContentRoot != null
                    ? new[] { ToSequencedItem(model.OwningWorkingTree.ContentRoot) }
                    : Enumerable.Empty<SequencedItem>());
        }

        public static ISequenceUniquenessStrategy<TreeNodeModel> TreeNode()
        {
            // Узлы сравниваются только с узлами той коллекции, в которой они находятся:
            // либо с узлами корня, либо с узлами родительского узла.
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
            // Листья имеют собственную коллекцию внутри TreeNodeModel и не конфликтуют по Sequence с дочерними узлами.
            return new SequenceUniquenessStrategy<TreeLeaveModel>(
                model => model.ParentNode?.ChildLeaves.Select(ToSequencedItem) ?? Enumerable.Empty<SequencedItem>());
        }

        public static ISequenceUniquenessStrategy<ElementAttributeModel> ElementAttribute()
        {
            // Для Sequence атрибутов проверяется вся коллекция атрибутов владельца, включая унаследованные.
            // Пользователь видит их в одном списке, поэтому одинаковый порядок у собственного и унаследованного
            // атрибута внутри одного владельца тоже считается конфликтом.
            return new SequenceUniquenessStrategy<ElementAttributeModel>(
                model => PolicyRuleModelQueries.GetAttributes(model.Owner).Select(ToSequencedItem));
        }

        private static SequencedItem ToSequencedItem(IShrubMemberModel model)
        {
            return new SequencedItem(model.Uuid, model.Sequence);
        }
    }
}
