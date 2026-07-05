namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Конфигурация MessagingConfig.
    /// </summary>
    public class MessagingConfig
    {
        /// <summary>
        /// Длительность сессии пользователя сообщений в секундах.
        /// </summary>
        public string? SessionDurability { get; set; }
    }
}
