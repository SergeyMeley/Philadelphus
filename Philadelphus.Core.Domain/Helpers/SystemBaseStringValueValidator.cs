using Philadelphus.Core.Domain.Entities.Enums;
using System.Globalization;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Проверяет строковое представление значения системного базового типа.
    /// </summary>
    /// <remarks>
    /// Сейчас системные листья хранят значение в строковом виде, поэтому этот helper является
    /// общей точкой проверки для доменных правил, работающих с <see cref="SystemBaseType" />.
    /// При добавлении нового системного типа сюда нужно добавить и проверку, и текст ожидаемого формата.
    /// </remarks>
    internal static class SystemBaseStringValueValidator
    {
        /// <summary>
        /// Проверяет, может ли строка быть значением указанного системного типа.
        /// </summary>
        /// <param name="type">Системный базовый тип, по которому выбирается формат проверки.</param>
        /// <param name="value">Проверяемое строковое значение.</param>
        /// <param name="expectedFormat">Описание ожидаемого формата для диагностического сообщения.</param>
        /// <returns>true, если значение соответствует системному типу; иначе false.</returns>
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
                SystemBaseType.FILE => SystemBaseFileValueAccessValidator.IsAvailable(value),
                _ => false,
            };
        }

        /// <summary>
        /// Возвращает человекочитаемое описание ожидаемого формата системного значения.
        /// </summary>
        /// <param name="type">Системный базовый тип.</param>
        /// <returns>Описание формата, подходящее для сообщения валидации.</returns>
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
                SystemBaseType.FILE => "путь к доступному локальному файлу; MinIO пока не поддерживается",
                _ => $"тип {type} не поддерживается этим правилом",
            };
        }

        /// <summary>
        /// Проверяет логическое значение с учетом стандартного .NET-формата и системных русских значений.
        /// </summary>
        /// <param name="value">Проверяемая строка.</param>
        /// <returns>true для true/false, Истина/Ложь; иначе false.</returns>
        private static bool IsBool(string value)
        {
            return bool.TryParse(value, out _)
                || string.Equals(value, "Истина", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Ложь", StringComparison.OrdinalIgnoreCase);
        }
    }
}
