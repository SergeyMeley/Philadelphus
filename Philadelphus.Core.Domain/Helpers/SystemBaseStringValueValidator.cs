using Philadelphus.Core.Domain.Entities.Enums;
using System.Globalization;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Проверяет и разбирает строковое представление значения системного базового типа.
    /// </summary>
    /// <remarks>
    /// Сейчас системные листья хранят значение в строковом виде, поэтому этот helper является
    /// общей точкой разбора для самих системных листьев и доменных правил, работающих с <see cref="SystemBaseType" />.
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
            return TryParse(type, value, out _, out expectedFormat);
        }

        /// <summary>
        /// Проверяет строку системного базового типа и возвращает ее доменное типизированное представление.
        /// </summary>
        /// <param name="type">Системный базовый тип, по которому выбирается формат проверки.</param>
        /// <param name="value">Проверяемое строковое значение.</param>
        /// <param name="typedValue">Типизированное значение, полученное из строки.</param>
        /// <param name="expectedFormat">Описание ожидаемого формата для диагностического сообщения.</param>
        /// <returns>true, если значение успешно приведено к типу; иначе false.</returns>
        public static bool TryParse(SystemBaseType type, string? value, out object? typedValue, out string expectedFormat)
        {
            expectedFormat = GetExpectedFormat(type);
            typedValue = null;

            if (value is null)
            {
                return false;
            }

            switch (type)
            {
                case SystemBaseType.STRING:
                case SystemBaseType.OBJECT:
                    typedValue = value;
                    return true;
                case SystemBaseType.INTEGER:
                    if (long.TryParse(value, out var integerValue))
                    {
                        typedValue = integerValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.NUMERIC:
                case SystemBaseType.FLOAT:
                    if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
                    {
                        typedValue = doubleValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.MONEY:
                    if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue))
                    {
                        typedValue = decimalValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.BOOL:
                    return TryParseBool(value, out typedValue);
                case SystemBaseType.DATETIME:
                    if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
                    {
                        typedValue = dateTimeValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.DATE:
                    if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateValue))
                    {
                        typedValue = dateValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.TIME:
                    if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeValue))
                    {
                        typedValue = timeValue;
                        return true;
                    }

                    return false;
                case SystemBaseType.FILE:
                    if (SystemBaseFileValueValidator.IsSupportedReference(value))
                    {
                        typedValue = value;
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
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
                SystemBaseType.FILE => "непустая ссылка на файл: локальный путь, file:// URI или URI внешнего хранилища",
                _ => $"тип {type} не поддерживается этим правилом",
            };
        }

        /// <summary>
        /// Разбирает логическое значение из стандартного .NET-формата или системных русских значений.
        /// </summary>
        /// <param name="value">Проверяемая строка.</param>
        /// <param name="typedValue">Логическое значение.</param>
        /// <returns>true, если строка распознана как boolean; иначе false.</returns>
        private static bool TryParseBool(string value, out object? typedValue)
        {
            if (bool.TryParse(value, out var boolValue))
            {
                typedValue = boolValue;
                return true;
            }

            if (string.Equals(value, "Истина", StringComparison.OrdinalIgnoreCase))
            {
                typedValue = true;
                return true;
            }

            if (string.Equals(value, "Ложь", StringComparison.OrdinalIgnoreCase))
            {
                typedValue = false;
                return true;
            }

            typedValue = null;
            return false;
        }
    }
}
