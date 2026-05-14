using System;
using System.Globalization;

namespace Philadelphus.Core.Domain.ImportExport.Excel
{
    internal static class ExcelRelationKeyHelper
    {
        public static bool AreEqual(string left, string right)
        {
            var normalizedLeft = Normalize(left);
            var normalizedRight = Normalize(right);

            if (string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase))
                return true;

            return TryParseNumber(normalizedLeft, out var leftNumber)
                && TryParseNumber(normalizedRight, out var rightNumber)
                && leftNumber == rightNumber;
        }

        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim()
                .Replace('\u00A0', ' ')
                .Replace('\t', ' ')
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            return normalized;
        }

        private static bool TryParseNumber(string value, out decimal result)
        {
            var compactValue = value
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);

            return decimal.TryParse(compactValue, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
                || decimal.TryParse(compactValue, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }
    }
}
