using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.OtherEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs
{
    public class MessageLogControlVM : ControlBaseVM
    {
        private readonly ObservableCollection<NotificationVM> _currentMessageLogAllNotifications = new ObservableCollection<NotificationVM>();
        private readonly ObservableCollection<NotificationVM> _currentMessageLogFilteredNotifications = new ObservableCollection<NotificationVM>();
        private bool _IsInfoMessagesVisible = true;
        private bool _IsWarningMessagesVisible = true;
        private bool _IsErrorMessagesVisible = true;
        public bool IsInfoMessagesVisible 
        { 
            get
            {
                return _IsInfoMessagesVisible;
            }
            set
            {
                _IsInfoMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsInfoMessagesVisible));
            }
        }
        public bool IsWarningMessagesVisible
        {
            get
            {
                return _IsWarningMessagesVisible;
            }
            set
            {
                _IsWarningMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsWarningMessagesVisible));
            }
        }
        public bool IsErrorMessagesVisible
        {
            get
            {
                return _IsErrorMessagesVisible;
            }
            set
            {
                _IsErrorMessagesVisible = value;
                FilterVisibleNotifications();
                OnPropertyChanged(nameof(IsErrorMessagesVisible));
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
            ILogger<ControlBaseVM> logger, 
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            CopyNotifications();
            FilterVisibleNotifications();
            SubscribeNotifications();
        }

        private void CopyNotifications()
        {
            foreach (var item in _notificationService.NotificationsHistory.ToList())
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _currentMessageLogAllNotifications.Add(new NotificationVM(item, _notificationService));
                    FilterVisibleNotifications();
                });
            }
        }
        private void SubscribeNotifications()
        {
            _notificationService.HistoryUpdated += (n) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var vm = new NotificationVM(n, _notificationService);
                    if (IsVisible(vm))
                    {
                        _currentMessageLogAllNotifications.Add(vm);
                        FilterVisibleNotifications();
                    }
                });
            };
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
            _currentMessageLogFilteredNotifications.Clear();
            foreach (var item in _currentMessageLogAllNotifications)
            {
                if (IsVisible(item))
                {
                    _currentMessageLogFilteredNotifications.Add(item);
                }
            }
            OnPropertyChanged(nameof(InfoMessagesCount));
            OnPropertyChanged(nameof(WarningMessagesCount));
            OnPropertyChanged(nameof(ErrorMessagesCount));
        }

        private bool IsVisible(NotificationVM notification)
        {
            return (IsErrorMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Error || notification.CriticalLevel == NotificationCriticalLevelModel.Alarm))
                || (IsWarningMessagesVisible && notification.CriticalLevel == NotificationCriticalLevelModel.Warning)
                || (IsInfoMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Info || notification.CriticalLevel == NotificationCriticalLevelModel.None));
        }
    }
}
