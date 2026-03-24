using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.OtherEntitiesVMs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs
{
    public class MessageLogControlVM : ControlBaseVM, IDisposable
    {
        private readonly ObservableCollection<NotificationVM> _currentMessageLogAllNotifications = new ObservableCollection<NotificationVM>();
        private readonly ObservableCollection<NotificationVM> _currentMessageLogFilteredNotifications = new ObservableCollection<NotificationVM>();
        private bool _isOkMessagesVisible = true;
        private bool _isInfoMessagesVisible = true;
        private bool _isWarningMessagesVisible = true;
        private bool _isErrorMessagesVisible = true;

        private bool _isSubscribedNotificationsHistoryUpdate;
        private bool _isSetedHandler;

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
                var visible = _currentMessageLogFilteredNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Ok)
                    .Count();
                var inTotal = _currentMessageLogAllNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Ok)
                    .Count();
                return $"{visible}/{inTotal}";
            }
        }
        public string InfoMessagesCount 
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Info 
                    || x.CriticalLevel == NotificationCriticalLevelModel.None)
                    .Count();
                var inTotal = _currentMessageLogAllNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Info
                    || x.CriticalLevel == NotificationCriticalLevelModel.None)
                    .Count();
                return $"{visible}/{inTotal}";
            }
        }
        public string WarningMessagesCount 
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Warning)
                    .Count();
                var inTotal = _currentMessageLogAllNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Warning)
                    .Count();
                return $"{visible}/{inTotal}";
            }
        }
        public string ErrorMessagesCount
        {
            get
            {
                var visible = _currentMessageLogFilteredNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Error
                    || x.CriticalLevel == NotificationCriticalLevelModel.Alarm)
                    .Count();
                var inTotal = _currentMessageLogAllNotifications.Where(x =>
                    x.CriticalLevel == NotificationCriticalLevelModel.Error
                    || x.CriticalLevel == NotificationCriticalLevelModel.Alarm)
                    .Count();
                return $"{visible}/{inTotal}";
            }
        }
        public ObservableCollection<NotificationVM> CurrentMessageLogFilteredNotifications
        {
            get
            {
                return _currentMessageLogFilteredNotifications;
            }
        }

        public string MessagingUserName { get => _notificationService.CurrentUser.NameWithNanoid; }


        public MessageLogControlVM(
            IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger logger, 
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            CopyNotificationsHistory();
        }

        internal bool CopyNotificationsHistory()
        {
            foreach (var item in _notificationService.NotificationsHistory.ToList())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _currentMessageLogAllNotifications.Add(new NotificationVM(item, _notificationService));
                });
            }
            FilterVisibleNotifications();

            return true;
        }

        internal bool SubscribeNotificationsHistoryUpdate()
        {
            if (_isSubscribedNotificationsHistoryUpdate)
                return false;

            _notificationService.HistoryUpdated += (n) => AddNewMessage(n);
            _isSubscribedNotificationsHistoryUpdate = true;

            _notificationService.SendTextMessage<MessageLogControlVM>(
                "Журнал сообщений подписан на все необработанные уведомления.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        internal bool SetHandler()
        {
            if (_isSetedHandler)
                return false;

            _notificationService.TextMessageHandler += mes => AddNewMessage(mes);
            _isSetedHandler = true;

            _notificationService.SendTextMessage<MessageLogControlVM>(
                "Обработчик текстовых сообщений назначен.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        private bool AddNewMessage(Notification notification)
        {
            if (_currentMessageLogAllNotifications.Any(x => x.Model == notification))
                return false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = new NotificationVM(notification, _notificationService);
                _currentMessageLogAllNotifications.Add(vm);
                FilterVisibleNotifications();
            }); 
            
            return true;
        }

        public RelayCommand ClearMessageLogCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _currentMessageLogAllNotifications.Clear();
                    FilterVisibleNotifications();
                });
            }
        }

        public RelayCommand SendTestNotificationCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _notificationService.SendTextMessage<MessageLogControlVM>("Тестовое уведомление", NotificationCriticalLevelModel.Info, NotificationTransmissionType.Broadcast);
                    FilterVisibleNotifications();
                });
            }
        }

        private void FilterVisibleNotifications()
        {
            var remItems = new List<NotificationVM>();
            foreach (var item in _currentMessageLogFilteredNotifications)
            {
                if (_currentMessageLogAllNotifications.Contains(item) == false)
                {
                    remItems.Add(item);
                }
                else if (IsVisible(item) == false)
                {
                    remItems.Add(item);
                }
            }
            foreach (var item in remItems)
            {
                _currentMessageLogFilteredNotifications.Remove(item);
            }
            foreach (var item in _currentMessageLogAllNotifications)
            {
                if (IsVisible(item) && _currentMessageLogFilteredNotifications.Contains(item) == false)
                {
                    var next = _currentMessageLogFilteredNotifications?.OrderBy(x => x.DateTime)?.FirstOrDefault(x => x.DateTime > item.DateTime);
                    if (next != null)
                    {
                        _currentMessageLogFilteredNotifications.Insert(_currentMessageLogFilteredNotifications.IndexOf(next), item);
                    }
                    else
                    {
                        _currentMessageLogFilteredNotifications.Add(item);
                    }
                }
            }

            OnPropertyChanged(nameof(OkMessagesCount));
            OnPropertyChanged(nameof(InfoMessagesCount));
            OnPropertyChanged(nameof(WarningMessagesCount));
            OnPropertyChanged(nameof(ErrorMessagesCount));
        }

        private bool IsVisible(NotificationVM notification)
        {
            return (IsErrorMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Error || notification.CriticalLevel == NotificationCriticalLevelModel.Alarm))
                || (IsWarningMessagesVisible && notification.CriticalLevel == NotificationCriticalLevelModel.Warning)
                || (IsInfoMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Info || notification.CriticalLevel == NotificationCriticalLevelModel.None)) 
                || (IsOkMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Ok));
        }

        public void Dispose()
        {
            if (_isSubscribedNotificationsHistoryUpdate)
            {
                _notificationService.HistoryUpdated -= (n) => AddNewMessage(n);
                _isSubscribedNotificationsHistoryUpdate = false;
            }
            if (_isSetedHandler)
            {
                _notificationService.TextMessageHandler -= mes => AddNewMessage(mes);
                _isSetedHandler = false;
            }
        }
    }
}
