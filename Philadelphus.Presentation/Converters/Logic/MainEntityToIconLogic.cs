using Philadelphus.Presentation.Enums;
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
                TreeRootVM => AppIcon.TreeRoot,
                TreeNodeVM => AppIcon.TreeNode,
                TreeLeaveVM => AppIcon.TreeLeaf,
                _ => AppIcon.Empty
            };
    }
}
