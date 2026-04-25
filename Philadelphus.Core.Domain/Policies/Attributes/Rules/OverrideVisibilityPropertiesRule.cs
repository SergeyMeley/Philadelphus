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

        public OverrideVisibilityPropertiesRule(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

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

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
