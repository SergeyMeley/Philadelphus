using Philadelphus.Presentation.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Сопоставление типа сущности рабочего дерева с AppIcon (без привязки к UI-фреймворку).
    /// </summary>
    public static class MainEntityToIconLogic
    {
        /// <summary>
        /// Возвращает иконку для типа сущности рабочего дерева.
        /// </summary>
        public static AppIcon ResolveIcon(object? value)
            => value switch
            {
                PhiladelphusRepositoryVM => AppIcon.RepositoryLogo,
                ShrubVM => AppIcon.Shrub,
                WorkingTreeVM => AppIcon.WorkingTree,
                TreeRootVM => AppIcon.TreeRoot,
                TreeNodeVM => AppIcon.TreeNode,
                TreeLeaveVM => AppIcon.TreeLeaf,
                PhiladelphusRepositoryModel => AppIcon.RepositoryLogo,
                ShrubModel => AppIcon.Shrub,
                WorkingTreeModel => AppIcon.WorkingTree,
                TreeRootModel => AppIcon.TreeRoot,
                TreeNodeModel => AppIcon.TreeNode,
                TreeLeaveModel => AppIcon.TreeLeaf,
                ElementAttributeModel => AppIcon.Attribute,
                _ => AppIcon.Empty
            };
    }
}
