using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services
{
    public static class NotificationService
    {
        public static NotificationHandler TextMessageHandler { get; set; }
        public static NotificationHandler ModalWindowHandler { get; set; }
        public static NotificationHandler PopUpWindowHandler { get; set; }
        public static NotificationHandler EmailHandler { get; set; }
        public static NotificationHandler SmsHandler { get; set; }
        public static NotificationHandler CallHandler { get; set; }

        public static ObservableCollection<Notification> Notifications { get; private set;  } = new ObservableCollection<Notification>();

        public static bool SendNotification(string text, NotificationCriticalLevel criticalLevel = NotificationCriticalLevel.Error, NotificationTypes type = NotificationTypes.TextMessage)
        {
            Notification notification = new Notification(text, criticalLevel);
            Notifications.Add(notification);
            return notification.TryInvokeHandler(type);
        }

        private static bool TryInvokeHandler(this Notification notification, NotificationTypes type)
        {
            NotificationHandler handler = null;

            switch (type)
            {
                case NotificationTypes.TextMessage:
                    handler = TextMessageHandler;
                    break;
                case NotificationTypes.ModalWindow:
                    handler = ModalWindowHandler;
                    break;
                case NotificationTypes.PopUpWindow:
                    handler = PopUpWindowHandler;
                    break;
                case NotificationTypes.Email:
                    handler = EmailHandler;
                    break;
                case NotificationTypes.Sms:
                    handler = SmsHandler;
                    break;
                case NotificationTypes.Call:
                    handler = CallHandler;
                    break;
                default:
                    break;
            }

            if (handler == null)
            {
                SendMissHandlerNotification();
                return false;
            }
            else
            {
                handler.Invoke(notification);
                return true;
            }
        }

        private static bool SendMissHandlerNotification()
        {
            Notification error = new Notification("Не задан требуемый обработчик уведомлений. Осуществляется попытка отправить с повышенным обработчиком", NotificationCriticalLevel.Error);

            Notifications.Add(error);

            for (int i = 0; i < Enum.GetValues(typeof(NotificationTypes)).Length; i++)
            {
                if (error.TryInvokeHandler((NotificationTypes)i))
                    return true;
            }
                
            return false;
        }
    }
}
