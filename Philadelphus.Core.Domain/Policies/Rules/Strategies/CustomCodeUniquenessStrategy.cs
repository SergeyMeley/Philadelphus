using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Фабрика стратегий проверки уникальности свойства CustomCode.
    /// </summary>
    internal static class CustomCodeUniquenessStrategy
    {
        public static ICustomCodeUniquenessStrategy<TreeRootModel> TreeRoot()
        {
            // Для любых элементов рабочего дерева CustomCode уникален в пределах всего WorkingTreeModel.
            return new CustomCodeUniquenessStrategy<TreeRootModel>(model => GetWorkingTreeMembers(model));
        }

        public static ICustomCodeUniquenessStrategy<TreeNodeModel> TreeNode()
        {
            // Для любых элементов рабочего дерева CustomCode уникален в пределах всего WorkingTreeModel.
            return new CustomCodeUniquenessStrategy<TreeNodeModel>(model => GetWorkingTreeMembers(model));
        }

        public static ICustomCodeUniquenessStrategy<TreeLeaveModel> TreeLeave()
        {
            // Для любых элементов рабочего дерева CustomCode уникален в пределах всего WorkingTreeModel.
            return new CustomCodeUniquenessStrategy<TreeLeaveModel>(model => GetWorkingTreeMembers(model));
        }

        public static ICustomCodeUniquenessStrategy<ElementAttributeModel> ElementAttribute()
        {
            // Для атрибута область уникальности ограничена текущим владельцем, но в нее входят
            // и собственные, и унаследованные атрибуты, потому что пользователь работает с ними
            // как с единой коллекцией атрибутов владельца.
            return new CustomCodeUniquenessStrategy<ElementAttributeModel>(
                model => PolicyRuleModelQueries.GetAttributes(model.Owner).Select(ToCustomCodeItem));
        }

        private static IEnumerable<CustomCodeItem> GetWorkingTreeMembers<T>(T model)
            where T : WorkingTreeMemberBaseModel<T>
        {
            // Собираем корень, все узлы и все листья дерева. Атрибуты сюда не входят:
            // для них используется отдельная стратегия ElementAttribute().
            if (model.OwningWorkingTree.ContentRoot != null)
                yield return ToCustomCodeItem(model.OwningWorkingTree.ContentRoot);

            foreach (var node in model.OwningWorkingTree.ContentNodes)
                yield return ToCustomCodeItem(node);

            foreach (var leave in model.OwningWorkingTree.ContentLeaves)
                yield return ToCustomCodeItem(leave);
        }

        private static CustomCodeItem ToCustomCodeItem<T>(WorkingTreeMemberBaseModel<T> model)
            where T : WorkingTreeMemberBaseModel<T>
        {
            return new CustomCodeItem(model.Uuid, model.CustomCode);
        }
    }
}
