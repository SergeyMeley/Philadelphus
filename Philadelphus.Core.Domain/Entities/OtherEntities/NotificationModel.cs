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
        public NotificationCriticalLevelModel CriticalLevel { get; }

        /// <summary>
        /// Текст уведомления
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Текст уведомления
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Время уведомления
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Уведомление
        /// </summary>
        /// <param name="text">Текст уведомления</param>
        /// <param name="criticalLevel">Уровень критичности уведомления</param>
        public NotificationModel(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            CriticalLevel = criticalLevel;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
