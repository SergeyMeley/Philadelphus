namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Уровень критичности уведомления
    /// </summary>
    public enum NotificationCriticalLevelModel
    {
        /// <summary>
        /// Нет типа
        /// </summary>
        None = 0,
        /// <summary>
        /// Информационное уведомление
        /// </summary>
        Info,
        /// <summary>
        /// Предупреждение
        /// </summary>
        Warning,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error,
        /// <summary>
        /// Тревога
        /// </summary>
        Alarm
    }
}
