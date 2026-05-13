using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Имя пользователя.
        /// </summary>
        public string UserName { get; }

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
        {
            UserUuid = userUuid;
            UserName = userName;
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
