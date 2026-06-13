using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Converters.Logic
{
    /// <summary>
    /// Чистая логика конвертера CriticalLevelToIconConverter (без привязки к UI-фреймворку).
    /// </summary>
    public static class CriticalLevelToIconLogic
    {
        /// <summary>
        /// Возвращает ключ ресурса иконки для уровня критичности; null, если значение не распознано.
        /// </summary>
        public static string? ResolveResourceKey(object? value)
            => value is NotificationCriticalLevelModel criticalLevel
                ? criticalLevel switch
                {
                    NotificationCriticalLevelModel.None => "imageEmpty",
                    NotificationCriticalLevelModel.Ok => "imageOk64_1",
                    NotificationCriticalLevelModel.Info => "imageInfo64_2",
                    NotificationCriticalLevelModel.Warning => "imageWarning64",
                    NotificationCriticalLevelModel.Error => "imageError64_3",
                    NotificationCriticalLevelModel.Alarm => "imageAlarm64",
                    _ => "imageEmpty"
                }
                : null;
    }
}
