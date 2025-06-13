using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.OtherEntities
{
    public class Notification
    {
        public NotificationCriticalLevel CriticalLevel { get; init; }
        public string Text { get; init; }
        public DateTime DateTime { get; init; }
        public Notification(string text, NotificationCriticalLevel criticalLevel)
        {
            CriticalLevel = criticalLevel;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
