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

        public RequiredOverrideValuePropertiesRule(
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

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
