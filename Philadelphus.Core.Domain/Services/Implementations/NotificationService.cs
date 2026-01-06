using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис управления уведомлениями
    /// </summary>
    public class NotificationService : INotificationService
    {
        /// <summary>
        /// Обработчик текстовых сообщений
        /// </summary>
        public NotificationHandler TextMessageHandler { get; set; }

        /// <summary>
        /// Обработчик модальных (блокирующих) окон
        /// </summary>
        public NotificationHandler ModalWindowHandler { get; set; }

        /// <summary>
        /// Обработчик всплывающих попап-окон
        /// </summary>
        public NotificationHandler PopUpWindowHandler { get; set; }

        /// <summary>
        /// Обработчик электронных писем
        /// </summary>
        public NotificationHandler EmailHandler { get; set; }

        /// <summary>
        /// Обработчик смс-сообщений
        /// </summary>
        public NotificationHandler SmsHandler { get; set; }

        /// <summary>
        /// Обработчик телефонных звонков
        /// </summary>
        public NotificationHandler CallHandler { get; set; }

        /// <summary>
        /// Коллекция сообщений
        /// </summary>
        public ObservableCollection<NotificationModel> Notifications { get; private set;  } = new ObservableCollection<NotificationModel>();

        /// <summary>
        /// Направить уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <param name="type">Тип уведомления</param>
        /// <returns></returns>
        public bool SendNotification(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error, NotificationTypesModel type = NotificationTypesModel.TextMessage)
        {
            NotificationModel notification = new NotificationModel(text, criticalLevel);
            Notifications.Add(notification);
            return TryInvokeHandler(notification, type);
        }

        /// <summary>
        /// Направить текстовое сообщение
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendTextMessage(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text , criticalLevel, NotificationTypesModel.TextMessage);
        }

        /// <summary>
        /// Вывести всплывающее попап-окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendPopUpWindow(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.PopUpWindow);
        }

        /// <summary>
        /// Вывести модальное (блокирующее) окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendModalWindow(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.ModalWindow);
        }


        /// <summary>
        /// Направить электронное письмо
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendEmail(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Email);
        }

        /// <summary>
        /// Направить смс-уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendSms(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Sms);
        }

        /// <summary>
        /// Направить телефонный звонок
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendCall(string text, NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error)
        {
            return SendNotification(text, criticalLevel, NotificationTypesModel.Call);
        }

        /// <summary>
        /// Попробывать вызвать обработчик
        /// </summary>
        /// <param name="notification">Уведомление</param>
        /// <param name="type">Тип обработчика</param>
        /// <returns></returns>
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

        /// <summary>
        /// Уведомить об отсутствии обработчика
        /// </summary>
        /// <returns></returns>
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
