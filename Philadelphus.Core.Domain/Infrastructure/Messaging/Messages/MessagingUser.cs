using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging.Messages
{
    public class MessagingUser
    {
        private static readonly TimeSpan _sessionDurability = TimeSpan.FromSeconds(30);
        public Guid UserUuid { get; }
        public string UserName { get; }
        public string Nanoid { get; set; } = NanoidDotNet.Nanoid.Generate(size: 6);
        public string NameWithNanoid { get => $"{UserName} ({Nanoid})"; }
        public Guid SessionUuid { get; private set; }
        public DateTime StartSessionDateTime { get; private set; }
        public bool IsActive 
        { 
            get
            {
                return DateTime.UtcNow >= StartSessionDateTime 
                    && DateTime.UtcNow <= StartSessionDateTime + _sessionDurability;
            }
        }
        public MessagingUser(Guid userUuid, string userName)
        {
            UserUuid = userUuid;
            UserName = userName;
            UpdateSession();
        }
        internal void UpdateSession()
        {
            SessionUuid = Guid.NewGuid();
            StartSessionDateTime = DateTime.UtcNow;
        }
        internal static int GetSessionDurability() => (int)_sessionDurability.TotalMilliseconds;
    }
}
