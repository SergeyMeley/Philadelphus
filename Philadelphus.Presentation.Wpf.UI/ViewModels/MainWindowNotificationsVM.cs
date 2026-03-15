using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs;
using System.Collections.ObjectModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class MainWindowNotificationsVM : ViewModelBase
    {
        private readonly ILogger<MainWindowNotificationsVM> _logger;
        private readonly INotificationService _notificationService;

        private PopUpNotificationsControlVM _popupVM;

        private Dictionary<Guid, MessageLogControlVM> MessageLogsVMs { get; } = new Dictionary<Guid, MessageLogControlVM>();
        public PopUpNotificationsControlVM PopupVM
        {
            get
            {
                return _popupVM;
            }
        }

        public ObservableCollection<Notification> AllMainWindowNotifications { get; } = new ObservableCollection<Notification>();
        public MainWindowNotificationsVM(
            PopUpNotificationsControlVM popupVM,
            ILogger<MainWindowNotificationsVM> logger,
            INotificationService notificationService)
        {
            _popupVM = popupVM;
            _logger = logger;
            _notificationService = notificationService;

            SetNotificationHandlers();

            CopyNotifications();
            SubscribeNotifications();
        }

        private void CopyNotifications()
        {
            foreach (var item in _notificationService.NotificationsHistory.ToList())
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AllMainWindowNotifications.Add(item);
                });
            }
        }
        private void SubscribeNotifications()
        {
            _notificationService.HistoryUpdated += (n) =>
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _notificationService.HistoryUpdated += (n) =>
                    {
                        AllMainWindowNotifications.Add(n);
                    };
                });
            };
        }

        private bool SetNotificationHandlers()
        {
            _notificationService.TextMessageHandler = notification =>
            {
                foreach (var vm in MessageLogsVMs)
                {

                }
                return true;
            };
            _notificationService.SendTextMessage<MainWindowNotificationsVM>(
                "Обработчик текстовых сообщений назначен.", 
                criticalLevel: NotificationCriticalLevelModel.Info);

            _notificationService.ModalWindowHandler = notification =>
            {
                MessageBoxImage image;
                switch (notification.CriticalLevel)
                {
                    case NotificationCriticalLevelModel.Info:
                        image = MessageBoxImage.Information;
                        break;
                    case NotificationCriticalLevelModel.Warning:
                        image = MessageBoxImage.Warning;
                        break;
                    case NotificationCriticalLevelModel.Error:
                        image = MessageBoxImage.Error;
                        break;
                    case NotificationCriticalLevelModel.Alarm:
                        image = MessageBoxImage.Error;
                        break;
                    default:
                        image = MessageBoxImage.Error;
                        break;
                }
                MessageBox.Show(
                    messageBoxText: notification.Text,
                    caption: notification.CriticalLevel.ToString(),
                    MessageBoxButton.OK, icon: image);
                return true;
            };
            _notificationService.SendTextMessage<MainWindowNotificationsVM>("Обработчик модальных окон назначен.", criticalLevel: NotificationCriticalLevelModel.Info);

            return true;
        }
    }
}
