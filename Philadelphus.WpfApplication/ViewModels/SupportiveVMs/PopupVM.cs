using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.WpfApplication.Models.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Philadelphus.WpfApplication.ViewModels.SupportiveVMs
{
    public class PopupVM : ViewModelBase
    {
        private TimeSpan _periodicity = new TimeSpan(hours: 0, minutes: 0, seconds: 3);
        private TimeSpan _lifeTime = new TimeSpan(hours: 0, minutes: 0, seconds: 2);
        private DateTime _lastUpdate;

        private static bool _isOpen;
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }

        private static List<NotificationModel> _notificationList = new List<NotificationModel>();
        public List<NotificationModel> NotificationList 
        {
            get
            {
                if (_isOpen)
                {
                    return _notificationList;
                }
                else
                {
                    return null;
                }
            }
            private set
            {
                _notificationList = value;
                OnPropertyChanged(nameof(NotificationList));
            }
        }

        private NotificationModel _lastNotification = new NotificationModel("Hello!");
        public NotificationModel LastNotification 
        { 
            get => _lastNotification; 
            set => _lastNotification = value; 
        }

        public bool StartReceivingNotifications()
        {
            _isOpen = true;
            OnPropertyChanged(nameof(IsOpen));
            var popupAddTask = new Task(CheckMessages);
            popupAddTask.Start();
            var popupRemoveTask = new Task(CheckLifeTime);
            popupRemoveTask.Start();
            return true;
        }

        private void CheckMessages()
        {
            while (true)
            {
                //var notification = new NotificationModel(DateTime.Now.ToString());
                //_lastNotification = notification;
                //OnPropertyChanged(nameof(LastNotification));
                //var newList = new List<NotificationModel>(_notificationList);
                //newList.Add(notification);
                //_notificationList = newList;
                ////_notificationList.Add(notification);
                //OnPropertyChanged(nameof(NotificationList));
                ////OnPropertyChanged(nameof(NotificationList.Collection));
                //////var notification = new NotificationModel(DateTime.Now.ToString());
                //SendNotification(notification);
                //_lastUpdate = DateTime.Now;
                //Thread.Sleep(_periodicity);
            }
        }

        private void CheckLifeTime()
        {
            while (true)
            {
                var newList = new List<NotificationModel>(_notificationList);
                foreach (var notification in _notificationList)
                {
                    var qwe = DateTime.Now - notification.DateTime;
                     if ((_lastUpdate - notification.DateTime).TotalSeconds > _lifeTime.TotalSeconds)
                    {
                        newList.Remove(notification);
                    }
                }
                _notificationList = newList;
                OnPropertyChanged(nameof(NotificationList));
                _lastUpdate = DateTime.Now;
                Thread.Sleep((int)Math.Min(_periodicity.TotalSeconds, _lifeTime.TotalSeconds));
            }
        }

        private bool SendNotification(NotificationModel notification)
        {
            var newList = new List<NotificationModel>(_notificationList);
            newList.Add(notification);
            _notificationList = newList;
            //_notificationList.Add(notification);
            //OnPropertyChanged(nameof(NotificationList));
            //OnPropertyChanged(nameof(NotificationList.Collection));
            return true;
        }
    }
}
