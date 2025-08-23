using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.OtherEntities
{
    public class NotificationModel
    {
        public NotificationCriticalLeveModel CriticalLevel { get; init; }
        public string Text { get; init; }
        public DateTime DateTime { get; init; }
        public NotificationModel(string text, NotificationCriticalLeveModel criticalLevel = NotificationCriticalLeveModel.Error)
        {
            CriticalLevel = criticalLevel;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
