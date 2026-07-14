using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Enums;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Сводит произвольное значение привязки к <see cref="AppIcon" />: уже готовый AppIcon,
    /// уровень критичности уведомления или сущность рабочего дерева.
    /// </summary>
    public static class IconResolver
    {
        /// <summary>
        /// Возвращает иконку для значения привязки.
        /// </summary>
        public static AppIcon Resolve(object? value)
            => value switch
            {
                AppIcon appIcon => appIcon,
                NotificationCriticalLevelModel => CriticalLevelToIconLogic.ResolveIcon(value),
                IMainEntityVM => MainEntityToIconLogic.ResolveIcon(value),
                IMainEntityModel => MainEntityToIconLogic.ResolveIcon(value),
                _ => AppIcon.Empty
            };
    }
}
