using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.OtherEntitiesVMs
{
    public class NotificationVM : ViewModelBase
    {
        private readonly Notification _model;
        private readonly INotificationService _service;

        public Notification Model { get => _model; }

        /// <summary>
        /// Тип уведомления
        /// </summary>
        public NotificationTypesModel NotificationType => _model.NotificationType;

        /// <summary>
        /// Уровень критичности уведомления
        /// </summary>
        public NotificationCriticalLevelModel CriticalLevel => _model.CriticalLevel;

        /// <summary>
        /// Код уведомления
        /// </summary>
        public string Code => _model.Code;

        /// <summary>
        /// Текст уведомления
        /// </summary>
        public string Text => _model.Text;

        /// <summary>
        /// Время уведомления
        /// </summary>
        public DateTime DateTime => _model.DateTime;

        /// <summary>
        /// Отправитель уведомления
        /// </summary>
        public MessagingUser SendingUser => _model.SendingUser;

        /// <summary>
        /// Источник уведомления
        /// </summary>
        public string Source => _model.Source;

        /// <summary>
        /// Отображаемое имя отправилетя
        /// </summary>
        public string SenderDisplayName
        { 
            get
            {
                if (_model.SendingUser.UserUuid == _service.CurrentUser.UserUuid)
                {
                    return "Текущий";
                }
                else
                {
                    return _model.SendingUser.NameWithNanoid;
                }
            }
        }

        public NotificationVM(
            Notification model,
            INotificationService service)
        {
            _model = model;
            _service = service;
        }
    }
}
