using System.Text.Json.Serialization;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging.Messages
{
    /// <summary>
    /// Представляет объект пользователя системы сообщений.
    /// </summary>
    public class MessagingUser
    {
        private static readonly TimeSpan _sessionDurability = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public Guid UserUuid { get; }

        /// <summary>
        /// Имя пользователя, заданное вручную.
        /// </summary>
        public string? ManualUserName { get; }

        /// <summary>
        /// Имя пользователя, определенное автоматически.
        /// </summary>
        public string AutomaticUserName { get; }

        /// <summary>
        /// Отображаемое имя пользователя без технических уточнений.
        /// </summary>
        public string DisplayUserName => string.IsNullOrWhiteSpace(ManualUserName)
            ? AutomaticUserName
            : ManualUserName;

        /// <summary>
        /// Имя пользователя для логов, аудита и передачи между клиентами.
        /// </summary>
        public string UserName => string.IsNullOrWhiteSpace(ManualUserName)
            ? AutomaticUserName
            : $"{ManualUserName} [{AutomaticUserName}]";

        /// <summary>
        /// Короткий уникальный идентификатор Nanoid.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public string Nanoid { get; set; } = NanoidDotNet.Nanoid.Generate(size: 6);

        /// <summary>
        /// Наименование с коротким уникальным идентификатором Nanoid.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public string NameWithNanoid { get => $"{UserName} ({Nanoid})"; }

        /// <summary>
        /// Отображаемое имя с коротким уникальным идентификатором Nanoid.
        /// </summary>
        public string DisplayNameWithNanoid => $"{DisplayUserName} ({Nanoid})";
        
        /// <summary>
        /// Уникальный идентификатор сессии.
        /// </summary>
        public Guid SessionUuid { get; private set; }

        /// <summary>
        /// Дата и время начала сессии.
        /// </summary>
        public DateTime StartSessionDateTime { get; private set; }

        public bool IsActive 
        { 
            get
            {
                return DateTime.UtcNow >= StartSessionDateTime 
                    && DateTime.UtcNow <= StartSessionDateTime + _sessionDurability;
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessagingUser" />.
        /// </summary>
        /// <param name="userUuid">Уникальный идентификатор пользователя.</param>
        /// <param name="userName">Имя пользователя.</param>
        public MessagingUser(Guid userUuid, string userName)
            : this(userUuid, null, userName)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessagingUser" />.
        /// </summary>
        /// <param name="userUuid">Уникальный идентификатор пользователя.</param>
        /// <param name="manualUserName">Имя пользователя, заданное вручную.</param>
        /// <param name="automaticUserName">Имя пользователя, определенное автоматически.</param>
        [JsonConstructor]
        public MessagingUser(Guid userUuid, string? manualUserName, string? automaticUserName, string? userName = null)
        {
            var normalizedAutomaticUserName = string.IsNullOrWhiteSpace(automaticUserName)
                ? userName
                : automaticUserName;

            ArgumentException.ThrowIfNullOrWhiteSpace(normalizedAutomaticUserName);

            UserUuid = userUuid;
            ManualUserName = string.IsNullOrWhiteSpace(manualUserName)
                ? null
                : manualUserName.Trim();
            AutomaticUserName = normalizedAutomaticUserName.Trim();
            UpdateSession();
        }

        internal void UpdateSession()
        {
            SessionUuid = Guid.CreateVersion7();
            StartSessionDateTime = DateTime.UtcNow;
        }

        internal static int GetSessionDurability() => (int)_sessionDurability.TotalMilliseconds;
    }
}
