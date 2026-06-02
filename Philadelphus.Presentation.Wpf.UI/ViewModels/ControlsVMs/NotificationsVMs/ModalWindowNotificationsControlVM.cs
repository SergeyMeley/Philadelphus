using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;

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

            var dialog = _serviceProvider.GetRequiredService<IDialogService>();
            var title = notification.CriticalLevel.ToString();

            switch (notification.CriticalLevel)
            {
                case NotificationCriticalLevelModel.Ok:
                    dialog.ShowInformation(notification.Text, title); 
                    break;
                case NotificationCriticalLevelModel.Info:
                    dialog.ShowInformation(notification.Text, title);
                    break;
                case NotificationCriticalLevelModel.Warning:
                    dialog.ShowWarning(notification.Text, title);
                    break;
                case NotificationCriticalLevelModel.Error:
                    dialog.ShowError(notification.Text, title);
                    break;
                case NotificationCriticalLevelModel.Alarm:
                    dialog.ShowError(notification.Text, title);
                    break;
                default:
                    dialog.ShowError(notification.Text, title);
                    break;
            }
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
