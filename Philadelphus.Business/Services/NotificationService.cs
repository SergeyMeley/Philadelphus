using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services
{
    public static class NotificationService
    {
        public static ObservableCollection<Notification> Notifications { get; private set;  } = new ObservableCollection<Notification>();
        public static bool SendNotification(string text, NotificationCriticalLevel criticalLevel = NotificationCriticalLevel.Error, NotificationTypes type = NotificationTypes.TextMessage)
        {
            Notification notification = new Notification(text, criticalLevel);
            Notifications.Add(notification);
            switch (type)
            {
                case NotificationTypes.TextMessage:
                    break;
                case NotificationTypes.ModalWindow:
                    
                    break;
                case NotificationTypes.PopUpWindow:

                    break;
                default:
                    break;
            }
            return true;
        }

    }
}
