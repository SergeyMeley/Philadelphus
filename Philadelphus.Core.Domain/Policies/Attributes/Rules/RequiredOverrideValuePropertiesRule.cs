using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее изменение значений атрибута, если он является собственным и абстрактным
    /// </summary>
    public class RequiredOverrideValuePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        private static readonly HashSet<string> _locked =
        [
            nameof(ElementAttributeModel.Value),
            nameof(ElementAttributeModel.Values)
        ];

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RequiredOverrideValuePropertiesRule" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public RequiredOverrideValuePropertiesRule(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Признак доступности чтения.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        /// <summary>
        /// Признак доступности записи.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (model.IsOwn && model.Override == OverrideType.Abstract)
            {
                if (_locked.Contains(prop))
                {
                    _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                        $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                        $"изменение значения свойства '{prop}' ограничено, т.к. атрибут требует переопределения наследниками.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);

                    return false;
                }    
            }

            return true;
        }

        /// <summary>
        /// Выполняет операцию OnRead.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Результат выполнения операции.</returns>
        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        /// <summary>
        /// Выполняет операцию OnWrite.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="oldValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
