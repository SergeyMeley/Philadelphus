using AutoMapper;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs
{
    /// <summary>
    /// Модель представления для уведомления.
    /// </summary>
    public class PopUpNotificationsControlVM : ControlBaseVM
    {
        private TimeSpan _periodicity = new TimeSpan(hours: 0, minutes: 0, seconds: 3);
        private TimeSpan _lifeTime = new TimeSpan(hours: 0, minutes: 0, seconds: 2);
        private DateTime _lastUpdate;

        private static bool _isOpen;

        /// <summary>
        /// Признак открытого состояния.
        /// </summary>
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }

        private static List<Notification> _notificationList = new List<Notification>();
        public List<Notification> NotificationList 
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

        //private TextNotification _lastNotification = new TextNotification("Hello!");

        //public TextNotification LastNotification 
        //{ 
        //    get => _lastNotification; 
        //    set => _lastNotification = value; 
        //}

        /// <summary>
        /// Выполняет операцию уведомления.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
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
                var newList = new List<Notification>(_notificationList);
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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PopUpNotificationsControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        public PopUpNotificationsControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
        }

        private bool SendNotification(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var newList = new List<Notification>(_notificationList);
            newList.Add(notification);
            _notificationList = newList;
            //_notificationList.Add(notification);
            //OnPropertyChanged(nameof(NotificationList));
            //OnPropertyChanged(nameof(NotificationList.Collection));
            return true;
        }
    }
}
