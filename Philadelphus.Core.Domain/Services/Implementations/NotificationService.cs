using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Infrastructure.Messaging;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Philadelphus.Core.Domain.Services.Implementations
{
    /// <summary>
    /// Сервис управления уведомлениями
    /// </summary>
    public class NotificationService : INotificationService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IMessageConsumer<Notification> _mainConsumer;
        private readonly IMessageProducer<Notification> _mainProducer;
        private readonly IMessageConsumer<MessagingUser> _consumerJoinedMessageConsumer;
        private Timer _registrationTimer;
        private Timer _cleanUsersTimer;
        private readonly List<Notification> _notificationsHistory = new List<Notification>();
        private bool _disposed = false;

        /// <summary>
        /// История уведомлений дополнена
        /// </summary>
        public event Action<Notification>? HistoryUpdated;

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public MessagingUser CurrentUser { get; }

        /// <summary>
        /// Активные получатели уведомлений
        /// </summary>
        public ObservableCollection<MessagingUser> ActiveUsers { get; } = new ObservableCollection<MessagingUser>();

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
        public IReadOnlyList<Notification> NotificationsHistory => _notificationsHistory.AsReadOnly();

        /// <summary>
        /// Вместимость истории уведомлений
        /// </summary>
        public int HistoryCapacity { get; set; } = int.MaxValue;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="NotificationService" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="consumerJoinedMessageConsumer">Потребитель сообщений о подключении потребителя.</param>
        /// <param name="consumerJoinedMessageProducer">Производитель сообщений о подключении потребителя.</param>
        /// <param name="mainConsumer">Основной потребитель сообщений.</param>
        /// <param name="mainProducer">Основной производитель сообщений.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public NotificationService(
            ILogger logger,
            IMessageConsumer<MessagingUser> consumerJoinedMessageConsumer,
            IMessageProducer<MessagingUser> consumerJoinedMessageProducer, 
            IMessageConsumer<Notification> mainConsumer,
            IMessageProducer<Notification> mainProducer,
            IOptions<MessagingConfig> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(consumerJoinedMessageConsumer);
            ArgumentNullException.ThrowIfNull(mainConsumer);
            ArgumentNullException.ThrowIfNull(mainProducer);
            ArgumentNullException.ThrowIfNull(consumerJoinedMessageProducer);

            _logger = logger
                .ForContext("SourceContext", "NotificationService");

            CurrentUser = new MessagingUser(
                Guid.CreateVersion7(), 
                options?.Value?.MessagingUserName ?? $"{Environment.UserDomainName}\\{Environment.UserName}");

            _consumerJoinedMessageConsumer = consumerJoinedMessageConsumer;
            _mainConsumer = mainConsumer;
            _mainProducer = mainProducer;

            StartAutoRegistrarion(consumerJoinedMessageProducer);

            StartAutoCheckingActiveConsumers(_consumerJoinedMessageConsumer);
            StartAutoCheckingNotifications(_mainConsumer);
        }

        /// <summary>
        /// Направить уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <param name="type">Тип уведомления</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendNotification<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel,
            NotificationTransmissionType transmissionType, 
            NotificationTypesModel type = NotificationTypesModel.TextMessage,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            ThrowIfDisposed();

            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            Notification notification = new Notification(
                text: text, 
                sendingUser: CurrentUser, 
                source: $"{typeof(TCallerClass).Name}.{method}",
                criticalLevel: criticalLevel,
                notificationType: type);

            switch (transmissionType)
            {
                case NotificationTransmissionType.Self:
                    _logger.Information($"Уведомление '{notification.Nanoid}' отправлено себе. Уведомление: {notification.ToString()}");
                    return ProcessNotification(notification);
                case NotificationTransmissionType.Broadcast:
                    _mainProducer.ProduceAsync(notification, default);
                    _logger.Information($"Уведомление '{notification.Nanoid}' отправлено всем. Уведомление: {notification.ToString()}");
                    return true;
                default:
                    break;
            }

            _logger.Error($"Уведомление '{notification.Nanoid}' не отправлено. Неизвестный тип передачи данных.");
            return false;
        }

        /// <summary>
        /// Направить текстовое сообщение
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendTextMessage<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.TextMessage,
                method: method,
                file: file);
        }

        /// <summary>
        /// Вывести всплывающее попап-окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendPopUpWindow<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.PopUpWindow,
                method: method,
                file: file);
        }

        /// <summary>
        /// Вывести модальное (блокирующее) окно
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendModalWindow<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.ModalWindow,
                method: method,
                file: file);
        }

        /// <summary>
        /// Направить электронное письмо
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendEmail<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.Email,
                method: method,
                file: file);
        }

        /// <summary>
        /// Направить смс-уведомление
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendSms<TCallerClass>(
            string text, 
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.Sms,
                method: method,
                file: file);
        }

        /// <summary>
        /// Направить телефонный звонок
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="criticalLevel">Уровень критичности</param>
        /// <returns>Результат выполнения операции.</returns>
        public bool SendCall<TCallerClass>(
            string text,
            NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error,
            NotificationTransmissionType transmissionType = NotificationTransmissionType.Self,
            [CallerMemberName] string method = null,
            [CallerFilePath] string file = null)
        {
            return SendNotification<TCallerClass>(
                text: text,
                criticalLevel: criticalLevel,
                transmissionType: transmissionType,
                type: NotificationTypesModel.Call,
                method: method,
                file: file);
        }

        /// <summary>
        /// Обработать (передать пользователю) полученное уведомление
        /// </summary>
        /// <param name="notification">Уведомление</param>
        private bool ProcessNotification(Notification notification)
        {
            ThrowIfDisposed();

            ArgumentNullException.ThrowIfNull(notification);

            _logger.Information($"Начало обработки полученного уведомления '{notification.Nanoid}'. Уведомление: {notification.ToString()}");

            _notificationsHistory.Add(notification);
            HistoryUpdated?.Invoke(notification);

            NotificationHandler handler = null;

            switch (notification.NotificationType)
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
                _logger.Error($"Обработка уведомления '{notification.Nanoid}' завершена с ошибкой. Не назначен обработчик.");
                SendMissHandlerNotification(notification.NotificationType.ToString(), $"{nameof(NotificationService)}.{nameof(ProcessNotification)}");
                return false;
            }
            else
            {
                if (handler.Invoke(notification))
                {
                    _logger.Information($"Обработка уведомления '{notification.Nanoid}' завершена корректно.");
                }
                else
                {
                    _logger.Error($"Обработка уведомления '{notification.Nanoid}' завершена с ошибкой.");
                }
                return true;
            }
        }

        /// <summary>
        /// Уведомить об отсутствии обработчика
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        private bool SendMissHandlerNotification(string handlerName, string source)
        {
            var notification = new Notification(
                text: $"Не задан требуемый обработчик уведомлений '{handlerName}'. Осуществляется попытка отправить с повышенным обработчиком.",
                sendingUser: CurrentUser,
                source: source,
                criticalLevel: NotificationCriticalLevelModel.Warning);

            _notificationsHistory.Add(notification);
            HistoryUpdated?.Invoke(notification);

            //for (int i = 0; i < Enum.GetValues(typeof(NotificationTypesModel)).Length; i++)
            //{
            //    if (error.TryInvokeHandler((NotificationTypesModel)i))
            //        return true;
            //}

            return false;
        }

        private bool StartAutoRegistrarion(IMessageProducer<MessagingUser> consumerJoinedMessageProducer)
        {
            ArgumentNullException.ThrowIfNull(consumerJoinedMessageProducer);

            _registrationTimer = new Timer(
               callback: _ => _ = Task.Run(async () =>
               {
                   try
                   {
                       await consumerJoinedMessageProducer.ProduceAsync(CurrentUser, default);
                   }
                   catch (Exception ex)
                   {
                       _logger.Error(ex, "Ошибка автоматической регистрации получателя уведомлений.");
                   }
               }),
               state: null,
               dueTime: 0,
               period: MessagingUser.GetSessionDurability());

            _logger.Information("Запущена автоматическая регистрация получателя уведомлений.");
            return true;
        }

        private bool StartAutoCheckingActiveConsumers(IMessageConsumer<MessagingUser> consumer)
        {
            ArgumentNullException.ThrowIfNull(consumer);

            consumer.MessageReceived += async (user, ct) =>
            {
                if (user?.IsActive ?? false)
                {
                    var au = ActiveUsers.SingleOrDefault(x => x.UserUuid == user.UserUuid);
                    if (au != null)
                    {
                        au.UpdateSession();
                        //SendTextMessage<NotificationService>($"{user.NameWithNanoid} [{user.UserUuid}] - сессия обновлена.", criticalLevel: NotificationCriticalLevelModel.Info);
                    }
                    else
                    {
                        ActiveUsers.Add(user);
                        SendTextMessage<NotificationService>($"{user.NameWithNanoid} [{user.UserUuid}] - начало сессии.", criticalLevel: NotificationCriticalLevelModel.Info);
                    }
                }
            };

            _cleanUsersTimer = new Timer(
               callback: _ => _ = Task.Run(() =>
               {
                   try
                   {
                       foreach (var user in ActiveUsers.ToList())
                       {
                           if (user.IsActive == false)
                           {
                               ActiveUsers.Remove(user);
                               SendTextMessage<NotificationService>($"{user.NameWithNanoid} [{user.UserUuid}] - конец сессии.", criticalLevel: NotificationCriticalLevelModel.Info);
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       _logger.Error(ex, "Ошибка автоматической проверки активных получателей уведомлений.");
                   }
               }),
               state: null,
               dueTime: 0,
               period: MessagingUser.GetSessionDurability());

            _logger.Information("Запущена автоматическая проверка активных получателей уведомлений.");
            return true;
        }

        private bool StartAutoCheckingNotifications(IMessageConsumer<Notification> consumer)
        {
            ArgumentNullException.ThrowIfNull(consumer);

            _mainConsumer.MessageReceived += async (notification, ct) =>
            {
                ProcessNotification(notification);
            };

            _logger.Information("Запущени автоматическое получение уведомлений.");
            return true;
        }

        /// <summary>
        /// Освобождение управляемых ресурсов
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Внутренний метод освобождения ресурсов
        /// </summary>
        /// <param name="disposing">Признак вызова из Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    _registrationTimer?.Dispose();
                    _cleanUsersTimer?.Dispose();
                    _logger.Information("NotificationService: Таймеры успешно освобождены");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "NotificationService: Ошибка при освобождении таймеров");
                }
            }

            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <summary>
        /// Финализатор для гарантированного освобождения ресурсов
        /// </summary>
        ~NotificationService()
        {
            Dispose(false);
        }
    }
}
