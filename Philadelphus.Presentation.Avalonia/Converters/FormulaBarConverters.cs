using System;
using System.Globalization;

using global::Avalonia.Data.Converters;
using global::Avalonia.Media;

using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Цвет текста сегмента подсветки формулы по <see cref="FormulaHighlightKind"/>.
    /// Avalonia-замена WPF DataTrigger'ов в строке формул.
    /// </summary>
    public sealed class FormulaHighlightKindToBrushConverter : IValueConverter
    {
        private static readonly IBrush Default = new SolidColorBrush(Color.Parse("#2B2B2B"));
        private static readonly IBrush Function = new SolidColorBrush(Color.Parse("#005A9E"));
        private static readonly IBrush Identifier = new SolidColorBrush(Color.Parse("#6A4C00"));
        private static readonly IBrush Number = new SolidColorBrush(Color.Parse("#098658"));
        private static readonly IBrush String = new SolidColorBrush(Color.Parse("#A31515"));
        private static readonly IBrush TreeLeaveReference = new SolidColorBrush(Color.Parse("#795E26"));
        private static readonly IBrush Parenthesis = new SolidColorBrush(Color.Parse("#5C2D91"));
        private static readonly IBrush Punctuation = new SolidColorBrush(Color.Parse("#666666"));
        private static readonly IBrush Operator = new SolidColorBrush(Color.Parse("#AF00DB"));
        private static readonly IBrush Error = new SolidColorBrush(Color.Parse("#B00020"));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is FormulaHighlightKind kind
                ? kind switch
                {
                    FormulaHighlightKind.Function => Function,
                    FormulaHighlightKind.Identifier => Identifier,
                    FormulaHighlightKind.Number => Number,
                    FormulaHighlightKind.String => String,
                    FormulaHighlightKind.TreeLeaveReference => TreeLeaveReference,
                    FormulaHighlightKind.Parenthesis => Parenthesis,
                    FormulaHighlightKind.Punctuation => Punctuation,
                    FormulaHighlightKind.Operator => Operator,
                    FormulaHighlightKind.Error => Error,
                    _ => Default,
                }
                : Default;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Насыщенность шрифта сегмента подсветки формулы (функции/операторы/ссылки — полужирные).
    /// </summary>
    public sealed class FormulaHighlightKindToFontWeightConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is FormulaHighlightKind.Function
                or FormulaHighlightKind.Operator
                or FormulaHighlightKind.TreeLeaveReference
                ? FontWeight.SemiBold
                : FontWeight.Normal;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Булево значение → кисть. Параметр: "ЦветTrue|ЦветFalse" (например "Transparent|Black").
    /// </summary>
    public sealed class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var flag = value is true;
            var parts = (parameter as string)?.Split('|');
            var colorText = parts is { Length: 2 }
                ? (flag ? parts[0] : parts[1])
                : (flag ? "Black" : "Transparent");

            return new SolidColorBrush(Color.Parse(colorText));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
