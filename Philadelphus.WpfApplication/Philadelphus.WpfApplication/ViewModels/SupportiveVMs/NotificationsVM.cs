using AutoMapper;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.Business.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.WpfApplication.ViewModels.SupportiveVMs
{
    public class NotificationsVM
    {
        private readonly ILogger<NotificationsVM> _logger;
        private readonly INotificationService _notificationService;

        private PopupVM _popupVM;
        public PopupVM PopupVM
        {
            get
            {
                if (_popupVM == null)
                {
                    _popupVM = new PopupVM();
                }
                return _popupVM;
            }
        }

        public ObservableCollection<NotificationModel> Notifications { get => _notificationService.Notifications; }
        public NotificationsVM(
            ILogger<NotificationsVM> logger,
            INotificationService notificationService)
        {
            _logger = logger;
            _notificationService = notificationService;

            SetNotificationHandlers();
        }
        private bool SetNotificationHandlers()
        {
            _notificationService.TextMessageHandler = SendText;
            bool SendText(NotificationModel notification)
            {
                return true;
            }
            _notificationService.SendNotification("Обработчик текстовых сообщений назначен.", criticalLevel: NotificationCriticalLevelModel.Info, type: NotificationTypesModel.TextMessage);

            _notificationService.ModalWindowHandler = SendModal;
            bool SendModal(NotificationModel notification)
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
                MessageBox.Show(messageBoxText: notification.Text, caption: notification.CriticalLevel.ToString(), MessageBoxButton.OK, icon: image);
                return true;
            }
            _notificationService.SendNotification("Обработчик модальных окон назначен.", criticalLevel: NotificationCriticalLevelModel.Info, type: NotificationTypesModel.TextMessage);

            return true;
        }
    }
}
