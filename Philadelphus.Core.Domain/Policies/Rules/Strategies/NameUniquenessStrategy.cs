using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Фабрика стратегий проверки уникальности свойства Name.
    /// </summary>
    internal static class NameUniquenessStrategy
    {
        public static INameUniquenessStrategy<WorkingTreeModel> WorkingTree()
        {
            return new NameUniquenessStrategy<WorkingTreeModel>(
                reservedPropertyTypes: Array.Empty<Type>(),
                getNamedItems: model => model.OwningShrub?.ContentWorkingTrees.Select(ToNamedItem) ?? Enumerable.Empty<NamedItem>());
        }

        public static INameUniquenessStrategy<TreeRootModel> TreeRoot()
        {
            return new NameUniquenessStrategy<TreeRootModel>(
                reservedPropertyTypes: GetReservedPropertyTypes<TreeRootModel>(),
                getNamedItems: model => GetWorkingTreeNamedItems(model.OwningWorkingTree));
        }

        public static INameUniquenessStrategy<TreeNodeModel> TreeNode()
        {
            return new NameUniquenessStrategy<TreeNodeModel>(
                reservedPropertyTypes: GetReservedPropertyTypes<TreeNodeModel>(),
                getNamedItems: model => GetParentNamedItems(model.Parent));
        }

        public static INameUniquenessStrategy<TreeLeaveModel> TreeLeave()
        {
            return new NameUniquenessStrategy<TreeLeaveModel>(
                reservedPropertyTypes: GetReservedPropertyTypes<TreeLeaveModel>(),
                getNamedItems: model => GetParentNamedItems(model.Parent));
        }

        public static INameUniquenessStrategy<ElementAttributeModel> ElementAttribute()
        {
            return new NameUniquenessStrategy<ElementAttributeModel>(
                reservedPropertyTypes: GetReservedPropertyTypes<ElementAttributeModel>(),
                getNamedItems: model => GetAttributeOwnerNamedItems(model.Owner));
        }

        private static IEnumerable<Type> GetReservedPropertyTypes<T>()
            where T : MainEntityBaseModel<T>
        {
            yield return typeof(T);

            if (typeof(T) == typeof(ElementAttributeModel))
            {
                yield return typeof(WorkingTreeMemberBaseModel<ElementAttributeModel>);
            }
            else if (typeof(T) == typeof(TreeRootModel))
            {
                yield return typeof(WorkingTreeMemberBaseModel<TreeRootModel>);
            }
            else if (typeof(T) == typeof(TreeNodeModel))
            {
                yield return typeof(WorkingTreeMemberBaseModel<TreeNodeModel>);
            }
            else if (typeof(T) == typeof(TreeLeaveModel))
            {
                yield return typeof(WorkingTreeMemberBaseModel<TreeLeaveModel>);
            }
        }

        private static IEnumerable<NamedItem> GetWorkingTreeNamedItems(WorkingTreeModel model)
        {
            if (model == null)
                yield break;

            if (model.ContentRoot != null)
                yield return ToNamedItem(model.ContentRoot);

            foreach (var item in model.ContentNodes)
                yield return ToNamedItem(item);

            foreach (var item in model.ContentLeaves)
                yield return ToNamedItem(item);

            foreach (var item in model.ContentAttributes)
                yield return ToNamedItem(item);
        }

        private static IEnumerable<NamedItem> GetParentNamedItems(IParentModel parent)
        {
            if (parent == null)
                yield break;

            foreach (var item in parent.Childs.Values.OfType<IMainEntityModel>())
                yield return ToNamedItem(item);

            if (parent is IOwnerModel owner)
            {
                foreach (var item in GetDirectAttributeNamedItems(owner))
                    yield return ToNamedItem(item);
            }
        }

        private static IEnumerable<NamedItem> GetAttributeOwnerNamedItems(IOwnerModel owner)
        {
            if (owner is IParentModel parent)
            {
                foreach (var item in GetParentNamedItems(parent))
                    yield return item;
            }

            if (owner is not IParentModel)
            {
                foreach (var item in GetDirectAttributeNamedItems(owner))
                    yield return ToNamedItem(item);
            }
        }

        private static IEnumerable<ElementAttributeModel> GetDirectAttributeNamedItems(IOwnerModel owner)
        {
            return PolicyRuleModelQueries.GetDirectAttributes(owner);
        }

        private static NamedItem ToNamedItem(IMainEntityModel model)
        {
            return new NamedItem(model.Uuid, model.Name);
        }
    }
}
