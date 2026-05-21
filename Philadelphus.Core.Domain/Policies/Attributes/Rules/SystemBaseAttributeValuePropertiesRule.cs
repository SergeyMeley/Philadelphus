using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Проверяет совместимость значения системного листа с системным типом атрибута.
    /// </summary>
    /// <remarks>
    /// Сам <see cref="SystemBaseTreeLeaveModel" /> уже гарантирует корректность значения для собственного
    /// <see cref="SystemBaseTreeLeaveModel.SystemBaseType" />. Это правило остается отдельным, потому что
    /// атрибут может ожидать другой системный тип, и тогда строковое значение листа нужно проверить уже
    /// относительно типа атрибута.
    /// </remarks>
    public class SystemBaseAttributeValuePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        public SystemBaseAttributeValuePropertiesRule(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (prop != nameof(ElementAttributeModel.Value)
                || model.ValueType is not SystemBaseTreeNodeModel systemBaseType
                || value is not SystemBaseTreeLeaveModel systemBaseValue)
            {
                return true;
            }

            var stringValue = systemBaseValue.StringValue;
            var isValid = SystemBaseStringValueValidator.IsValid(systemBaseType.SystemBaseType, stringValue, out var expectedFormat);

            if (isValid)
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseAttributeValuePropertiesRule>(
                $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                $"значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType.SystemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
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
