using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Core.Domain.Entities.OtherEntities
{
    public class NotificationModel
    {
        public NotificationCriticalLevelModel CriticalLevel { get; init; }
        public string Text { get; init; }
        public DateTime DateTime { get; init; }
        public NotificationModel(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            CriticalLevel = criticalLevel;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
