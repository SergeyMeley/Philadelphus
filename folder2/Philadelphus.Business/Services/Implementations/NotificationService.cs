using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Handlers;
using Philadelphus.Business.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        public NotificationHandler TextMessageHandler { get; set; }
        public NotificationHandler ModalWindowHandler { get; set; }
        public NotificationHandler PopUpWindowHandler { get; set; }
        public NotificationHandler EmailHandler { get; set; }
        public NotificationHandler SmsHandler { get; set; }
        public NotificationHandler CallHandler { get; set; }

        public ObservableCollection<NotificationModel> Notifications { get; private set;  } = new ObservableCollection<NotificationModel>();

        public bool SendNotification(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error, NotificationTypesModel type = NotificationTypesModel.TextMessage)
        {
            NotificationModel notification = new NotificationModel(text, criticalLevel);
            Notifications.Add(notification);
            return TryInvokeHandler(notification, type);
        }
        public bool SendTextMessage(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text , criticalLevel, NotificationTypesModel.TextMessage);
        }
        public bool SendPopUpWindow(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.PopUpWindow);
        }
        public bool SendModalWindow(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.ModalWindow);
        }
        public bool SendEmail(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Email);
        }
        public bool SendSms(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Sms);
        }
        public bool SendCall(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Call);
        }

        private bool TryInvokeHandler(NotificationModel notification, NotificationTypesModel type)
        {
            NotificationHandler handler = null;

            switch (type)
            {
                case NotificationTypesModel.TextMessage:
                    handler = TextMessageHandler;
                    break;
                case NotificationTypesModel.PopUpWindow:
                    handler = PopUpWindowHandler;
                    break;
                case NotificationTypesModel.ModalWindow:
                    handler = ModalWindowHandler;
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

        private bool SendMissHandlerNotification()
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
