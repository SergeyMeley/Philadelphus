using Confluent.Kafka;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    public class MainWindowNotificationsVM : ViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly MessageLogControlVM _messageLogControlVM;
        private readonly PopUpNotificationsControlVM _popupControlVM;

        public MessageLogControlVM MessageLogControlVM
        {
            get
            {
                return _messageLogControlVM;
            }
        }
        public PopUpNotificationsControlVM PopupVM
        {
            get
            {
                return _popupControlVM;
            }
        }
        public MainWindowNotificationsVM(
            MessageLogControlVM messageLogControlVM,
            PopUpNotificationsControlVM popupControlVM,
            INotificationService notificationService)
        {
             _notificationService = notificationService;

            _messageLogControlVM = messageLogControlVM;
            _popupControlVM = popupControlVM;

            SetNotificationHandlers();
        }

        private bool SetNotificationHandlers()
        {
            MessageLogControlVM.SetTextMessageHandler();

            _notificationService.ModalWindowHandler = notification =>
            {
                MessageBoxImage image;
                switch (notification.CriticalLevel)
                {
                    case NotificationCriticalLevelModel.Ok:
                        image = MessageBoxImage.None;
                        break;
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

            MessageLogControlVM.SubscribeNotificationsHistoryUpdate();

            return true;
        }
    }
}
