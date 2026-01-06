using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Handlers;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис управления уведомлениями
    /// </summary>
    public interface INotificationService
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
        public ObservableCollection<NotificationModel> Notifications { get; }

        /// <summary>
        /// Направить уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <param name="type">Тип уведомления</param>
        /// <returns></returns>
        public bool SendNotification(string text, NotificationCriticalLevelModel criticalLevel, NotificationTypesModel type);

        /// <summary>
        /// Направить текстовое сообщение
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendTextMessage(string text, NotificationCriticalLevelModel criticalLevel);

        /// <summary>
        /// Вывести всплывающее попап-окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendPopUpWindow(string text, NotificationCriticalLevelModel criticalLevel);

        /// <summary>
        /// Вывести модальное (блокирующее) окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendModalWindow(string text, NotificationCriticalLevelModel criticalLevel);

        /// <summary>
        /// Направить электронное письмо
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendEmail(string text, NotificationCriticalLevelModel criticalLevel);

        /// <summary>
        /// Направить смс-уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendSms(string text, NotificationCriticalLevelModel criticalLevel);

        /// <summary>
        /// Направить телефонный звонок
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendCall(string text, NotificationCriticalLevelModel criticalLevel);
    }
}
