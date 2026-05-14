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
    /// <summary>
    /// Модель представления для уведомления.
    /// </summary>
    public class MainWindowNotificationsVM : ViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly MessageLogControlVM _messageLogControlVM;
        private readonly PopUpNotificationsControlVM _popupControlVM;
        private readonly ModalWindowNotificationsControlVM _modalControlVM;

        public MessageLogControlVM MessageLogControlVM
        {
            get
            {
                return _messageLogControlVM;
            }
        }
        public PopUpNotificationsControlVM PopupControlVM
        {
            get
            {
                return _popupControlVM;
            }
        }
        public ModalWindowNotificationsControlVM ModalControlVM
        {
            get
            {
                return _modalControlVM;
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindowNotificationsVM" />.
        /// </summary>
        /// <param name="messageLogControlVM">Параметр messageLogControlVM.</param>
        /// <param name="popupControlVM">Параметр popupControlVM.</param>
        /// <param name="modalControlVM">Параметр modalControlVM.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public MainWindowNotificationsVM(
            MessageLogControlVM messageLogControlVM,
            PopUpNotificationsControlVM popupControlVM,
            ModalWindowNotificationsControlVM modalControlVM,
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(messageLogControlVM);
            ArgumentNullException.ThrowIfNull(popupControlVM);
            ArgumentNullException.ThrowIfNull(modalControlVM);
            ArgumentNullException.ThrowIfNull(notificationService);

            _notificationService = notificationService;

            _messageLogControlVM = messageLogControlVM;
            _popupControlVM = popupControlVM;
            _modalControlVM = modalControlVM;

            SetNotificationHandlers();
        }

        private bool SetNotificationHandlers()
        {
            MessageLogControlVM.SetHandler();
            ModalControlVM.SetHandler();

            MessageLogControlVM.SubscribeNotificationsHistoryUpdate();

            return true;
        }
    }
}
