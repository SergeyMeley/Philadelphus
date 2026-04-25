using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее изменение значений некоторых свойств для унаследованных атрибутов
    /// </summary>
    public class NonOwnAttributePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        private static readonly HashSet<string> _mustBeInherited =
        [
            nameof(ElementAttributeModel.Name),
            nameof(ElementAttributeModel.Description),
            nameof(ElementAttributeModel.ValueType),
            nameof(ElementAttributeModel.IsCollectionValue),
            nameof(ElementAttributeModel.Visibility)
        ];

        private static readonly HashSet<string> _canBeInherited =
        [
            nameof(ElementAttributeModel.Name),
            nameof(ElementAttributeModel.Description),
            nameof(ElementAttributeModel.ValueType),
            nameof(ElementAttributeModel.IsCollectionValue),
            nameof(ElementAttributeModel.Value),
            nameof(ElementAttributeModel.Values),
            nameof(ElementAttributeModel.Visibility)
        ];

        public NonOwnAttributePropertiesRule(
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
            if (prop == nameof(model.IsOwn))
                return true;

            var result = model.IsOwn || (_mustBeInherited.Contains(prop) == false);

            if (result == false)
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"изменение значения свойства '{prop}' ограничено, т.к. атрибут не является собственным.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
            }

            return result;
        }

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            if (prop == nameof(model.IsOwn))
                return value;

            // Если атрибут НЕ собственный и значение может или обязано быть унаследовано
            if (model.IsOwn == false && (_canBeInherited.Contains(prop) || _mustBeInherited.Contains(prop)))
            {

                // Если значение свойства обязано быть унаследовано ИЛИ еще не заполнено у текущего атрибута, берем с родителя
                if (_mustBeInherited.Contains(prop) || value == default)
                {
                    return model?.InheritedAttributeFromParent?.GetType().GetProperty(prop)?.GetValue(model?.InheritedAttributeFromParent);
                }
            }

            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue) 
        { 
        }
    }
}