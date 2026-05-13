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
    /// Правило, ограничивающее создание приватного абстрактного атрибута
    /// </summary>
    public class OverrideVisibilityPropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="OverrideVisibilityPropertiesRule" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public OverrideVisibilityPropertiesRule(
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
            if (prop == nameof(ElementAttributeModel.Override) 
                && (OverrideType)value == OverrideType.Abstract)
            {
                if (model.Visibility == VisibilityScope.Private)
                {
                    _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                        $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                        $"изменение значения свойства '{prop}' ограничено, т.к. требующий переопределения атрибут не может быть скрыт от дочерних узлов.",
                        criticalLevel: NotificationCriticalLevelModel.Warning);

                    return false;
                }
            }

            if (prop == nameof(ElementAttributeModel.Visibility) 
                && (VisibilityScope)value == VisibilityScope.Private)
            {
                if (model.Override == OverrideType.Abstract)
                {
                    _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                        $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                        $"изменение значения свойства '{prop}' ограничено, т.к. скрытый от дочерних узлов атрибут не может быть переопределен.",
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
