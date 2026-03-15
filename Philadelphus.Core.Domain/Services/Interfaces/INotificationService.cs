using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис управления уведомлениями
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// История уведомлений дополнена
        /// </summary>
        public event Action<Notification>? HistoryUpdated;

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public MessagingUser CurrentUser { get; }

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
        /// История уведомлений
        /// </summary>
        public IReadOnlyList<Notification> NotificationsHistory { get; }

        /// <summary>
        /// Вместимость истории уведомлений
        /// </summary>
        public int HistoryCapacoty { get; set; }

        /// <summary>
        /// Направить уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <param name="type">Тип уведомления</param>
        /// <returns></returns>
        public bool SendNotification<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel,
            NotificationTransmissionType transmissionType,
            NotificationTypesModel type = NotificationTypesModel.TextMessage,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Направить текстовое сообщение
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendTextMessage<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Вывести всплывающее попап-окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendPopUpWindow<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Вывести модальное (блокирующее) окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendModalWindow<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Направить электронное письмо
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendEmail<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Направить смс-уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendSms<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);

        /// <summary>
        /// Направить телефонный звонок
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns></returns>
        public bool SendCall<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null);
    }
}
