using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Сопоставление уровня критичности уведомления с AppIcon (без привязки к UI-фреймворку).
    /// </summary>
    public static class CriticalLevelToIconLogic
    {
        /// <summary>
        /// Возвращает иконку для уровня критичности уведомления.
        /// </summary>
        public static AppIcon ResolveIcon(object? value)
            => value is NotificationCriticalLevelModel criticalLevel
                ? criticalLevel switch
                {
                    NotificationCriticalLevelModel.None => AppIcon.Empty,
                    NotificationCriticalLevelModel.Ok => AppIcon.StatusOk,
                    NotificationCriticalLevelModel.Info => AppIcon.StatusInfo,
                    NotificationCriticalLevelModel.Warning => AppIcon.StatusWarning,
                    NotificationCriticalLevelModel.Error => AppIcon.StatusError,
                    NotificationCriticalLevelModel.Alarm => AppIcon.StatusAlarm,
                    _ => AppIcon.Empty
                }
                : AppIcon.Empty;
    }
}
