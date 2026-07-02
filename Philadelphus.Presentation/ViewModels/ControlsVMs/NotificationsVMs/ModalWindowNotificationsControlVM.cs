using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Handlers;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Serilog;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs.NotificationsVMs
{
    /// <summary>
    /// Модель представления для уведомления.
    /// </summary>
    public class ModalWindowNotificationsControlVM : ControlBaseVM, IDisposable
    {
        private bool _isSetedHandler;
        private NotificationHandler? _modalWindowHandler;

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
            IApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            
        }

        public bool SetHandler()
        {
            if (_isSetedHandler)
                return false;

            _modalWindowHandler = ShowModal;
            _notificationService.ModalWindowHandler = _modalWindowHandler;

            _isSetedHandler = true;

            _notificationService.SendTextMessage<ModalWindowNotificationsControlVM>(
                "Обработчик модальных окон назначен.",
                criticalLevel: NotificationCriticalLevelModel.Ok);

            return true;
        }

        private bool ShowModal(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);

            _ = ShowModalAsync(notification);
            return true;
        }

        private async Task ShowModalAsync(Notification notification)
        {
            var dialog = _serviceProvider.GetRequiredService<IDialogService>();
            var title = notification.CriticalLevel.ToString();

            try
            {
                switch (notification.CriticalLevel)
                {
                    case NotificationCriticalLevelModel.Ok:
                    case NotificationCriticalLevelModel.Info:
                        await dialog.ShowInformationAsync(notification.Text, title);
                        break;
                    case NotificationCriticalLevelModel.Warning:
                        await dialog.ShowWarningAsync(notification.Text, title);
                        break;
                    case NotificationCriticalLevelModel.Error:
                    case NotificationCriticalLevelModel.Alarm:
                    default:
                        await dialog.ShowErrorAsync(notification.Text, title);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка показа модального уведомления.");
            }
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            if (_isSetedHandler)
            {
                if (_modalWindowHandler != null)
                {
                    _notificationService.ModalWindowHandler -= _modalWindowHandler;
                    _modalWindowHandler = null;
                }

                _isSetedHandler = false;
            }
        }
    }
}
