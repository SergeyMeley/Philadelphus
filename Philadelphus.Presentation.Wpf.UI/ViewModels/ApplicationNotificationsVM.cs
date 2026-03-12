using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs;
using System.Collections.ObjectModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class ApplicationNotificationsVM : ViewModelBase
    {
        private readonly ILogger<ApplicationNotificationsVM> _logger;
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

        public ObservableCollection<NotificationModel> AllApplicationNotifications { get => _notificationService.Notifications; }
        public ApplicationNotificationsVM(
            PopUpNotificationsControlVM popupVM,
            ILogger<ApplicationNotificationsVM> logger,
            INotificationService notificationService)
        {
            _popupVM = popupVM;
            _logger = logger;
            _notificationService = notificationService;

            SetNotificationHandlers();
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
            _notificationService.SendNotification("Обработчик текстовых сообщений назначен.", criticalLevel: NotificationCriticalLevelModel.Info, type: NotificationTypesModel.TextMessage);

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
            _notificationService.SendTextMessage("Обработчик модальных окон назначен.", criticalLevel: NotificationCriticalLevelModel.Info);

            return true;
        }
    }
}
