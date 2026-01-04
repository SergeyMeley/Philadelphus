using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Handlers;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    public interface INotificationService
    {
        public NotificationHandler TextMessageHandler { get; set; }
        public NotificationHandler ModalWindowHandler { get; set; }
        public NotificationHandler PopUpWindowHandler { get; set; }
        public NotificationHandler EmailHandler { get; set; }
        public NotificationHandler SmsHandler { get; set; }
        public NotificationHandler CallHandler { get; set; }
        public ObservableCollection<NotificationModel> Notifications { get; }
        public bool SendNotification(string text, NotificationCriticalLevelModel criticalLevel, NotificationTypesModel type);
        public bool SendTextMessage(string text, NotificationCriticalLevelModel criticalLevel);
        public bool SendPopUpWindow(string text, NotificationCriticalLevelModel criticalLevel);
        public bool SendModalWindow(string text, NotificationCriticalLevelModel criticalLevel);
        public bool SendEmail(string text, NotificationCriticalLevelModel criticalLevel);
        public bool SendSms(string text, NotificationCriticalLevelModel criticalLevel);
        public bool SendCall(string text, NotificationCriticalLevelModel criticalLevel);
    }
}
