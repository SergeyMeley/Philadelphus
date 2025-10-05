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

        public static ObservableCollection<NotificationModel> Notifications { get; private set;  } = new ObservableCollection<NotificationModel>();

        public static bool SendNotification(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error, NotificationTypesModel type = NotificationTypesModel.TextMessage)
        {
            NotificationModel notification = new NotificationModel(text, criticalLevel);
            Notifications.Add(notification);
            return notification.TryInvokeHandler(type);
        }

        private static bool TryInvokeHandler(this NotificationModel notification, NotificationTypesModel type)
        {
            NotificationHandler handler = null;

            switch (type)
            {
                case NotificationTypesModel.TextMessage:
                    handler = TextMessageHandler;
                    break;
                case NotificationTypesModel.ModalWindow:
                    handler = ModalWindowHandler;
                    break;
                case NotificationTypesModel.PopUpWindow:
                    handler = PopUpWindowHandler;
                    break;
                case NotificationTypesModel.Email:
                    handler = EmailHandler;
                    break;
                case NotificationTypesModel.Sms:
                    handler = SmsHandler;
                    break;
                case NotificationTypesModel.Call:
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
            NotificationModel error = new NotificationModel("Не задан требуемый обработчик уведомлений. Осуществляется попытка отправить с повышенным обработчиком", NotificationCriticalLevelModel.Error);

            Notifications.Add(error);

            //for (int i = 0; i < Enum.GetValues(typeof(NotificationTypesModel)).Length; i++)
            //{
            //    if (error.TryInvokeHandler((NotificationTypesModel)i))
            //        return true;
            //}
                
            return false;
        }
    }
}
