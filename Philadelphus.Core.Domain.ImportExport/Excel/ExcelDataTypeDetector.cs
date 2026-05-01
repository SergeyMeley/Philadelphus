using System.Globalization;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    public class ExcelDataTypeDetector : IExcelDataTypeDetector
    {
        public string DetermineBestDataType(IEnumerable<string?> values)
        {
            var normalizedValues = values
                .Where(value => string.IsNullOrWhiteSpace(value) == false)
                .Select(value => value!.Trim())
                .ToList();

            if (normalizedValues.Count == 0)
                return "Текст";

            if (normalizedValues.All(TryParseInt64Flexible))
                return "Целое число";

            if (normalizedValues.All(TryParseDecimalFlexible))
            {
                var hasFractionalNotation = normalizedValues.Any(HasFractionalNotation);
                var hasPureIntegers = normalizedValues.Any(TryParseInt64Flexible);

                if (hasFractionalNotation && hasPureIntegers == false)
                    return "Дробное число";

                return "Число";
            }

            return "Текст";
        }

        public bool IsValueCompatibleWithDataType(string value, string dataTypeNodeName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            return dataTypeNodeName switch
            {
                "Целое число" => TryParseInt64Flexible(value),
                "Число" => TryParseDecimalFlexible(value),
                "Дробное число" => TryParseDecimalFlexible(value) && HasFractionalNotation(value),
                _ => true
            };
        }

        private static bool TryParseInt64Flexible(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalizedValue = value.Trim();
            return long.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.CurrentCulture, out _)
                || long.TryParse(normalizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        private static bool TryParseDecimalFlexible(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalizedValue = value.Trim();
            return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.CurrentCulture, out _)
                || decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _);
        }

        private static bool HasFractionalNotation(string value)
        {
            var normalizedValue = value.Trim();
            return normalizedValue.Contains('.') || normalizedValue.Contains(',');
        }
    }
}
