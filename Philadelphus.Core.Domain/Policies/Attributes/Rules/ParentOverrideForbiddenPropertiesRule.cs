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
    /// Правило, ограничивающее изменение значений свойств атрибута, для которого запрещего переопределение
    /// </summary>
    public class ParentOverrideForbiddenPropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ParentOverrideForbiddenPropertiesRule" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public ParentOverrideForbiddenPropertiesRule(
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
            // Если атрибут НЕ собственный и переопределение запрещено родителем, то изменение значений свойств атрибута запрещено
            if (model.IsOwn == false 
                && model.InheritedAttributeFromParent?.Override == OverrideType.Sealed)
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"изменение значения свойства '{prop}' ограничено, т.к. переопределение ограничено родительским элементом.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                return false;
            }

            // Если атрибут унаследован от НЕ абстрактного атрибута, он не может стать абстрактным
            if (model.IsOwn == false
                && prop == nameof(model.Override))
            {
                if (model.InheritedAttributeFromParent is { Override: not OverrideType.Abstract }
                    && (OverrideType)value == OverrideType.Abstract)
                {
                    _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                        $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                        $"изменение значения свойства '{prop}' ограничено, т.к. ограничение возможности переопределения у наследника не может быть жестче, чем у родителя.",
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
            // Если атрибут НЕ собственный и переопределение запрещено родителем, разрешить его у наследников нельзя
            if (prop == nameof(model.Override)
                && model.IsOwn == false
                && model.InheritedAttributeFromParent?.Override == OverrideType.Sealed)
            {
                return OverrideType.Sealed;
            }

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
