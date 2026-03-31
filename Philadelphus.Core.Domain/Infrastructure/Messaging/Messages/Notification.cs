using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging.Messages
{
    /// <summary>
    /// Уведомление
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Короткий идентификатор
        /// </summary>
        public object Nanoid { get; } = NanoidDotNet.Nanoid.Generate(size: 5);

        /// <summary>
        /// Тип уведомления
        /// </summary>
        public NotificationTypesModel NotificationType { get; }

        /// <summary>
        /// Уровень критичности уведомления
        /// </summary>
        public NotificationCriticalLevelModel CriticalLevel { get; }

        /// <summary>
        /// Код уведомления
        /// </summary>
        public string Code { get; init; }

        /// <summary>
        /// Текст уведомления
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Время уведомления
        /// </summary>
        public DateTime DateTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Отправитель уведомления
        /// </summary>
        public MessagingUser SendingUser { get; }

        /// <summary>
        /// Источник уведомления
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Уведомление
        /// </summary>
        /// <param name="text">Текст уведомления</param>
        /// <param name="criticalLevel">Уровень критичности уведомления</param>
        public Notification(
            string text, 
            MessagingUser sendingUser,
            string source,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTypesModel notificationType = NotificationTypesModel.TextMessage)
        {
            Text = text;
            SendingUser = sendingUser;
            Source = source;
            CriticalLevel = criticalLevel;
            NotificationType = notificationType;
        }

        public override string ToString()
        {
            return $"" +
                $"nanoid - '{Nanoid}'; " +
                $"код - '{Code}'; " +
                $"источник - '{Source}'; " +
                $"тип - '{NotificationType}'; " +
                $"критичность - '{CriticalLevel}'; " +
                $"текст: '{Text}'";
        }
    }
}
