using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Entities.OtherEntities
{
    /// <summary>
    /// Уведомление
    /// </summary>
    public class NotificationModel
    {
        /// <summary>
        /// Уровень критичности уведомления
        /// </summary>
        public NotificationCriticalLevelModel CriticalLevel { get; init; }

        /// <summary>
        /// Текст уведомления
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Время уведомления
        /// </summary>
        public DateTime DateTime { get; init; }

        /// <summary>
        /// Уведомление
        /// </summary>
        /// <param name="text">Ткст уведомления</param>
        /// <param name="criticalLevel">Уровень критичности уведомления</param>
        public NotificationModel(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            CriticalLevel = criticalLevel;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
