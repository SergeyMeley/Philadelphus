using Philadelphus.Core.Domain.Entities.Enums;
using System.Globalization;

namespace Philadelphus.Core.Domain.Helpers
{
    internal static class SystemBaseStringValueValidator
    {
        public static bool IsValid(SystemBaseType type, string? value, out string expectedFormat)
        {
            expectedFormat = GetExpectedFormat(type);

            if (value is null)
            {
                return false;
            }

            return type switch
            {
                SystemBaseType.STRING or SystemBaseType.OBJECT => true,
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

        public static string GetExpectedFormat(SystemBaseType type)
        {
            return type switch
            {
                SystemBaseType.STRING or SystemBaseType.OBJECT => "любое строковое значение, включая пустую строку",
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
