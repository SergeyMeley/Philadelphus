using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Policies.Builders;
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
            if (prop == nameof(ElementAttributeModel.Value))
            {
                return CanWriteValue(model, value as TreeLeaveModel);
            }

            if (prop == nameof(ElementAttributeModel.ValueType))
            {
                return CanWriteValueType(model, value as TreeNodeModel);
            }

            return true;
        }

        private bool CanWriteValue(ElementAttributeModel model, TreeLeaveModel? value)
        {
            if (SystemBaseAttributeValueCompatibilityValidator.IsCompatible(
                model.ValueType,
                value,
                out var systemBaseType,
                out var stringValue,
                out var expectedFormat))
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseAttributeValuePropertiesRule>(
                $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                $"значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        private bool CanWriteValueType(ElementAttributeModel model, TreeNodeModel? valueType)
        {
            if (model.IsCollectionValue)
            {
                foreach (var value in model.Values)
                {
                    if (TryResolveCompatibleValue(
                        valueType,
                        value,
                        out _,
                        out var systemBaseType,
                        out var stringValue,
                        out var expectedFormat) == false)
                    {
                        SendValueTypeChangeError(model, stringValue, systemBaseType, expectedFormat);
                        return false;
                    }
                }

                return true;
            }

            if (TryResolveCompatibleValue(
                valueType,
                model.Value,
                out _,
                out var singleSystemBaseType,
                out var singleStringValue,
                out var singleExpectedFormat))
            {
                return true;
            }

            SendValueTypeChangeError(model, singleStringValue, singleSystemBaseType, singleExpectedFormat);
            return false;
        }

        private void SendValueTypeChangeError(
            ElementAttributeModel model,
            string? stringValue,
            SystemBaseType? systemBaseType,
            string expectedFormat)
        {
            _notificationService.SendTextMessage<SystemBaseAttributeValuePropertiesRule>(
                $"Нельзя сменить тип атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}]: " +
                $"уже заданное значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }

        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
            if (prop != nameof(ElementAttributeModel.ValueType)
                || newValue is not TreeNodeModel valueType)
            {
                return;
            }

            if (model.IsCollectionValue)
            {
                var values = model.Values
                    .Select(value => ResolveCompatibleValueOrCurrent(valueType, value))
                    .GroupBy(value => value.Uuid)
                    .Select(x => x.First())
                    .ToList();

                model.ClearValuesCollection();
                foreach (var value in values)
                {
                    model.TryAddValueToValuesCollection(value);
                }

                return;
            }

            model.Value = ResolveCompatibleValueOrCurrent(valueType, model.Value);
        }

        private TreeLeaveModel ResolveCompatibleValueOrCurrent(TreeNodeModel valueType, TreeLeaveModel value)
        {
            return TryResolveCompatibleValue(
                valueType,
                value,
                out var resolvedValue,
                out _,
                out _,
                out _)
                ? resolvedValue ?? value
                : value;
        }

        private bool TryResolveCompatibleValue(
            TreeNodeModel? valueType,
            TreeLeaveModel? value,
            out TreeLeaveModel? resolvedValue,
            out SystemBaseType? systemBaseType,
            out string? stringValue,
            out string expectedFormat)
        {
            resolvedValue = value;
            systemBaseType = null;
            stringValue = null;
            expectedFormat = string.Empty;

            if (valueType is not SystemBaseTreeNodeModel systemBaseNode
                || value is not SystemBaseTreeLeaveModel systemBaseValue)
            {
                return true;
            }

            systemBaseType = systemBaseNode.SystemBaseType;
            stringValue = systemBaseValue.StringValue;
            if (SystemBaseStringValueValidator.IsValid(systemBaseType.Value, stringValue, out expectedFormat) == false)
            {
                return false;
            }

            if (systemBaseValue.ParentNode?.Uuid == systemBaseNode.Uuid)
            {
                return true;
            }

            resolvedValue = GetOrCreateCompatibleSystemBaseLeave(systemBaseNode, systemBaseValue);
            return resolvedValue != null;
        }

        private SystemBaseTreeLeaveModel? GetOrCreateCompatibleSystemBaseLeave(
            SystemBaseTreeNodeModel targetNode,
            SystemBaseTreeLeaveModel sourceValue)
        {
            if (targetNode.SystemBaseType == SystemBaseType.BOOL)
            {
                return ResolveBoolLeave(targetNode, sourceValue.StringValue);
            }

            var existing = targetNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => string.Equals(x.StringValue, sourceValue.StringValue, StringComparison.Ordinal));
            if (existing != null)
            {
                return existing;
            }

            var result = new SystemBaseTreeLeaveModel(
                Guid.CreateVersion7(),
                targetNode,
                targetNode.OwningWorkingTree,
                targetNode.SystemBaseType,
                _notificationService,
                PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));

            result.StringValue = sourceValue.StringValue;
            return result;
        }

        private static SystemBaseTreeLeaveModel? ResolveBoolLeave(
            SystemBaseTreeNodeModel targetNode,
            string stringValue)
        {
            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.BOOL, stringValue, out var typedValue, out _) == false
                || typedValue is not bool boolValue)
            {
                return null;
            }

            return targetNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => x.TypedValue is bool existingBoolValue && existingBoolValue == boolValue);
        }
    }
}
