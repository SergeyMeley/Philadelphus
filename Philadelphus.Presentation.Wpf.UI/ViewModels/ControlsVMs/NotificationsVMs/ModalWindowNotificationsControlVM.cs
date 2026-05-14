using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs
{
    /// <summary>
    /// Модель представления для уведомления.
    /// </summary>
    public class ModalWindowNotificationsControlVM : ControlBaseVM, IDisposable
    {
        private bool _isSetedHandler;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ModalWindowNotificationsControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        public ModalWindowNotificationsControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            
        }

        internal bool SetHandler()
        {
            if (_isSetedHandler)
                return false;

            _notificationService.ModalWindowHandler = notification => ShowModal(notification);

            _isSetedHandler = true;

            _notificationService.SendTextMessage<ModalWindowNotificationsControlVM>(
                "Обработчик модальных окон назначен.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        private bool ShowModal(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

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
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_isSetedHandler)
            {
                _notificationService.ModalWindowHandler -= (n) => ShowModal(n);
                _isSetedHandler = false;
            }
        }
    }
}
