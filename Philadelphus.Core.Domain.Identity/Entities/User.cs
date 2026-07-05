namespace Philadelphus.Core.Domain.Identity.Entities
{
    /// <summary>
    /// Пользователь приложения.
    /// </summary>
    public class User
    {
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
        /// Имя пользователя для логов, аудита и передачи между сборками.
        /// </summary>
        public string UserName => string.IsNullOrWhiteSpace(ManualUserName)
            ? AutomaticUserName
            : $"{ManualUserName} [{AutomaticUserName}]";

        /// <summary>
        /// Короткий уникальный идентификатор.
        /// </summary>
        public string Nanoid { get; } = Guid.NewGuid().ToString("N")[..6];

        /// <summary>
        /// Наименование с коротким уникальным идентификатором.
        /// </summary>
        public string NameWithNanoid => $"{UserName} ({Nanoid})";

        public User(Guid userUuid, string? manualUserName, string automaticUserName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(automaticUserName);

            UserUuid = userUuid;
            ManualUserName = string.IsNullOrWhiteSpace(manualUserName)
                ? null
                : manualUserName.Trim();
            AutomaticUserName = automaticUserName.Trim();
        }
    }
}
