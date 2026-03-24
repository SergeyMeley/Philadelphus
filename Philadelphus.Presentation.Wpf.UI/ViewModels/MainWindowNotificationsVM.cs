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
        public MainWindowNotificationsVM(
            MessageLogControlVM messageLogControlVM,
            PopUpNotificationsControlVM popupControlVM,
            ModalWindowNotificationsControlVM modalControlVM,
            INotificationService notificationService)
        {
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
