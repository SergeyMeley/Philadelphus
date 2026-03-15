using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Configurations
{
    public class MessagingConfig
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string MessagingUserName { get; set; }

        /// <summary>
        /// Продолжительность сессии чтения сообщений
        /// </summary>
        public int SessionDurability { get; set; } = 60;
    }
}
