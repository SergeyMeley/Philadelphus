using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Globalization;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Validates string values assigned through system base leaves to system base attribute types.
    /// </summary>
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
            var isValid = IsValid(systemBaseType.SystemBaseType, stringValue, out var expectedFormat);

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

        private static bool IsValid(SystemBaseType type, string? value, out string expectedFormat)
        {
            expectedFormat = GetExpectedFormat(type);

            if (value is null)
            {
                return false;
            }

            return type switch
            {
                SystemBaseType.STRING => true,
                SystemBaseType.INTEGER => long.TryParse(value, out _),
                SystemBaseType.NUMERIC or SystemBaseType.FLOAT =>
                    double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
                SystemBaseType.MONEY =>
                    decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
                SystemBaseType.BOOL => IsBool(value),
                SystemBaseType.DATETIME => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                SystemBaseType.DATE => DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                SystemBaseType.TIME => TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                _ => false,
            };
        }

        private static string GetExpectedFormat(SystemBaseType type)
        {
            return type switch
            {
                SystemBaseType.STRING => "любое строковое значение, включая пустую строку",
                SystemBaseType.INTEGER => "целое число в диапазоне Int64",
                SystemBaseType.NUMERIC or SystemBaseType.FLOAT =>
                    "дробное число в invariant culture, например 1234.56",
                SystemBaseType.MONEY =>
                    "денежное значение в invariant culture, например 1234.56",
                SystemBaseType.BOOL => "true/false или системные значения 'Истина'/'Ложь'",
                SystemBaseType.DATETIME => "дата и время, распознаваемые DateTimeOffset.TryParse",
                SystemBaseType.DATE => "дата, распознаваемая DateOnly.TryParse",
                SystemBaseType.TIME => "время, распознаваемое TimeOnly.TryParse",
                _ => $"тип {type} не поддерживается этим правилом",
            };
        }

        private static bool IsBool(string value)
        {
            return bool.TryParse(value, out _)
                || string.Equals(value, "Истина", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Ложь", StringComparison.OrdinalIgnoreCase);
        }
    }
}
