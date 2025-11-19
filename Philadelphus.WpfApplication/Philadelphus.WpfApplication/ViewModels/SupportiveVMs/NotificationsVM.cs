using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.WpfApplication.ViewModels.SupportiveVMs
{
    public class NotificationsVM
    {
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
        public NotificationsVM()
        {
            SetNotificationHandlers();
        }
        private bool SetNotificationHandlers()
        {
            NotificationService.TextMessageHandler = SendText;
            bool SendText(NotificationModel notification)
            {
                return true;
            }
            NotificationService.SendNotification("Обработчик текстовых сообщений назначен.", criticalLevel: NotificationCriticalLevelModel.Info, type: NotificationTypesModel.TextMessage);

            NotificationService.ModalWindowHandler = SendModal;
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
            NotificationService.SendNotification("Обработчик модальных окон назначен.", criticalLevel: NotificationCriticalLevelModel.Info, type: NotificationTypesModel.TextMessage);

            return true;
        }
    }
}
