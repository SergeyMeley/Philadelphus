using AutoMapper;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs
{
    public class MessageLogControlVM : ControlBaseVM
    {
        private readonly ObservableCollection<NotificationModel> _currentMessageLogAllNotifications = new ObservableCollection<NotificationModel>();
        private readonly ObservableCollection<NotificationModel> _currentMessageLogFilteredNotifications = new ObservableCollection<NotificationModel>();
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
        public ObservableCollection<NotificationModel> CurrentMessageLogFilteredNotifications
        {
            get
            {
                return _currentMessageLogFilteredNotifications;
            }
        }
        public MessageLogControlVM(
            IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger<ControlBaseVM> logger, 
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            foreach (var item in _notificationService.Notifications)
            {
                _currentMessageLogAllNotifications.Add(item);
            }
            FilterVisibleNotifications();

            _notificationService.Notifications.CollectionChanged += (s, e) => 
            {
                var item = _notificationService.Notifications.LastOrDefault();
                if (item != null)
                {
                    if (IsVisible(item))
                    {
                        _currentMessageLogAllNotifications.Add(item);
                        FilterVisibleNotifications();
                    }
                }
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
                    _notificationService.SendTextMessage("Тестовое уведомление", NotificationCriticalLevelModel.Info);
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

        private bool IsVisible(NotificationModel notification)
        {
            return (IsErrorMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Error || notification.CriticalLevel == NotificationCriticalLevelModel.Alarm))
                || (IsWarningMessagesVisible && notification.CriticalLevel == NotificationCriticalLevelModel.Warning)
                || (IsInfoMessagesVisible && (notification.CriticalLevel == NotificationCriticalLevelModel.Info || notification.CriticalLevel == NotificationCriticalLevelModel.None));
        }
    }
}
