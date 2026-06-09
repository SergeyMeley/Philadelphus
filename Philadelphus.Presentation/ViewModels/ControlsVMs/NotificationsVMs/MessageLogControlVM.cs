using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.OtherEntitiesVMs;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs.NotificationsVMs
{
    /// <summary>
    /// Модель представления для элемента управления.
    /// </summary>
    public class MessageLogControlVM : ControlBaseVM, IDisposable
    {
        private readonly ObservableCollection<NotificationVM> _currentMessageLogAllNotifications = new ObservableCollection<NotificationVM>();
        private readonly ObservableCollection<NotificationVM> _currentMessageLogFilteredNotifications = new ObservableCollection<NotificationVM>();
        private readonly ConcurrentQueue<Notification> _pendingNotifications = new ConcurrentQueue<Notification>();
        private readonly CancellationTokenSource _flushTimerCts = new();

        private bool _isOkMessagesVisible = true;
        private bool _isInfoMessagesVisible = true;
        private bool _isWarningMessagesVisible = true;
        private bool _isErrorMessagesVisible = true;

        private bool _isSubscribedNotificationsHistoryUpdate;
        private bool _isSetedHandler;
        private Action<Notification>? _historyUpdatedHandler;
        private NotificationHandler? _textMessageHandler;

        public bool IsOkMessagesVisible
        {
            get
            {
                return _isOkMessagesVisible;
            }
            set
            {
                _isOkMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsOkMessagesVisible));
            }
        }

        public bool IsInfoMessagesVisible
        {
            get
            {
                return _isInfoMessagesVisible;
            }
            set
            {
                _isInfoMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsInfoMessagesVisible));
            }
        }

        public bool IsWarningMessagesVisible
        {
            get
            {
                return _isWarningMessagesVisible;
            }
            set
            {
                _isWarningMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsWarningMessagesVisible));
            }
        }

        public bool IsErrorMessagesVisible
        {
            get
            {
                return _isErrorMessagesVisible;
            }
            set
            {
                _isErrorMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsErrorMessagesVisible));
            }
        }

        public string OkMessagesCount
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Count(x => x.CriticalLevel == NotificationCriticalLevelModel.Ok);
                var inTotal = _currentMessageLogAllNotifications.Count(x => x.CriticalLevel == NotificationCriticalLevelModel.Ok);
                return $"{visible}/{inTotal}";
            }
        }

        public string InfoMessagesCount
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Count(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Info
                    || x.CriticalLevel == NotificationCriticalLevelModel.None);
                var inTotal = _currentMessageLogAllNotifications.Count(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Info
                    || x.CriticalLevel == NotificationCriticalLevelModel.None);
                return $"{visible}/{inTotal}";
            }
        }
        public string WarningMessagesCount
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Count(x => x.CriticalLevel == NotificationCriticalLevelModel.Warning);
                var inTotal = _currentMessageLogAllNotifications.Count(x => x.CriticalLevel == NotificationCriticalLevelModel.Warning);
                return $"{visible}/{inTotal}";
            }
        }
        public string ErrorMessagesCount
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Count(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Error
                    || x.CriticalLevel == NotificationCriticalLevelModel.Alarm);
                var inTotal = _currentMessageLogAllNotifications.Count(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Error
                    || x.CriticalLevel == NotificationCriticalLevelModel.Alarm);
                return $"{visible}/{inTotal}";
            }
        }

        /// <summary>
        /// Уведомление.
        /// </summary>
        public ObservableCollection<NotificationVM> CurrentMessageLogFilteredNotifications => _currentMessageLogFilteredNotifications;

        /// <summary>
        /// Пользователь системы сообщений.
        /// </summary>
        public string MessagingUserName => _notificationService.CurrentUser.NameWithNanoid;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MessageLogControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        public MessageLogControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _ = RunFlushTimerAsync(_flushTimerCts.Token);

            CopyNotificationsHistory();
        }

        internal bool CopyNotificationsHistory()
        {
            var history = _notificationService.NotificationsHistory.ToList();
            _serviceProvider.GetRequiredService<IDispatcherService>().BeginInvoke(() =>
            {
                foreach (var item in history)
                {
                    _currentMessageLogAllNotifications.Add(new NotificationVM(item, _notificationService));
                }

                FilterVisibleNotifications();
            });

            return true;
        }

        public bool SubscribeNotificationsHistoryUpdate()
        {
            if (_isSubscribedNotificationsHistoryUpdate)
                return false;

            _historyUpdatedHandler = AddNewMessageFromHistory;
            _notificationService.HistoryUpdated += _historyUpdatedHandler;
            _isSubscribedNotificationsHistoryUpdate = true;

            _notificationService.SendTextMessage<MessageLogControlVM>(
                "Журнал сообщений подписан на все необработанные уведомления.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        public bool SetHandler()
        {
            if (_isSetedHandler)
                return false;

            _textMessageHandler = AddNewMessage;
            _notificationService.TextMessageHandler += _textMessageHandler;
            _isSetedHandler = true;

            _notificationService.SendTextMessage<MessageLogControlVM>(
                "Обработчик текстовых сообщений назначен.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        private void AddNewMessageFromHistory(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            AddNewMessage(notification);
        }

        private bool AddNewMessage(Notification notification)
        {
            if (notification == null)
                return false;

            _pendingNotifications.Enqueue(notification);
            return true;
        }

        private async Task RunFlushTimerAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            try
            {
                while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
                {
                    if (!_pendingNotifications.IsEmpty)
                        _serviceProvider.GetRequiredService<IDispatcherService>().Invoke(FlushPendingNotifications);
                }
            }
            catch (OperationCanceledException) { }
        }

        private void FlushPendingNotifications()
        {
            var addedAny = false;
            while (_pendingNotifications.TryDequeue(out var notification))
            {
                if (_currentMessageLogAllNotifications.Any(x => x.Model == notification))
                    continue;

                _currentMessageLogAllNotifications.Add(new NotificationVM(notification, _notificationService));
                addedAny = true;
            }

            if (addedAny)
                FilterVisibleNotifications();
        }

        public RelayCommand ClearMessageLogCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    DrainPendingNotifications();
                    _currentMessageLogAllNotifications.Clear();
                    FilterVisibleNotifications();
                });
            }
        }

        public RelayCommand SendTestNotificationCommand
        {
            get
            {
                return new RelayCommand(_ =>
                {
                    _notificationService.SendTextMessage<MessageLogControlVM>(
                        "Тестовое уведомление",
                        NotificationCriticalLevelModel.Info,
                        NotificationTransmissionType.Broadcast);
                    FilterVisibleNotifications();
                });
            }
        }

        private void FilterVisibleNotifications()
        {
            _currentMessageLogFilteredNotifications.Clear();

            foreach (var item in _currentMessageLogAllNotifications
                .Where(IsVisible)
                .OrderBy(x => x.DateTime))
            {
                _currentMessageLogFilteredNotifications.Add(item);
            }

            OnPropertyChanged(nameof(OkMessagesCount));
            OnPropertyChanged(nameof(InfoMessagesCount));
            OnPropertyChanged(nameof(WarningMessagesCount));
            OnPropertyChanged(nameof(ErrorMessagesCount));
        }

        private void DrainPendingNotifications()
        {
            while (_pendingNotifications.TryDequeue(out _))
            {
            }
        }

        private bool IsVisible(NotificationVM notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            return (IsErrorMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Error || notification.CriticalLevel == NotificationCriticalLevelModel.Alarm))
                || (IsWarningMessagesVisible && notification.CriticalLevel == NotificationCriticalLevelModel.Warning)
                || (IsInfoMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Info || notification.CriticalLevel == NotificationCriticalLevelModel.None))
                || (IsOkMessagesVisible && notification.CriticalLevel == NotificationCriticalLevelModel.Ok);
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            _flushTimerCts.Cancel();
            _flushTimerCts.Dispose();

            if (_isSubscribedNotificationsHistoryUpdate)
            {
                if (_historyUpdatedHandler != null)
                {
                    _notificationService.HistoryUpdated -= _historyUpdatedHandler;
                }

                _isSubscribedNotificationsHistoryUpdate = false;
            }

            if (_isSetedHandler)
            {
                if (_textMessageHandler != null)
                {
                    _notificationService.TextMessageHandler -= _textMessageHandler;
                }

                _isSetedHandler = false;
            }
        }
    }
}
