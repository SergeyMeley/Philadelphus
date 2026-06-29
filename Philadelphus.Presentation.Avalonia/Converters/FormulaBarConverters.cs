using System;
using System.Globalization;

using global::Avalonia;
using global::Avalonia.Data.Converters;
using global::Avalonia.Media;
using global::Avalonia.Styling;

using Philadelphus.Presentation.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Цвет текста сегмента подсветки формулы по <see cref="FormulaHighlightKind"/>.
    /// Avalonia-замена WPF DataTrigger'ов в строке формул.
    /// Палитра тема-зависимая: светлая — тёмные «VS»-цвета на светлом фоне,
    /// тёмная — яркие «VS Code Dark»-цвета на тёмном фоне (иначе текст нечитаем).
    /// Перекраска применяется при следующей пересборке сегментов (на ввод/смену курсора).
    /// </summary>
    public sealed class FormulaHighlightKindToBrushConverter : IValueConverter
    {
        // Светлая тема (тёмный текст на светлом фоне).
        private static readonly IBrush LightDefault = new SolidColorBrush(Color.Parse("#2B2B2B"));
        private static readonly IBrush LightFunction = new SolidColorBrush(Color.Parse("#005A9E"));
        private static readonly IBrush LightIdentifier = new SolidColorBrush(Color.Parse("#6A4C00"));
        private static readonly IBrush LightNumber = new SolidColorBrush(Color.Parse("#098658"));
        private static readonly IBrush LightString = new SolidColorBrush(Color.Parse("#A31515"));
        private static readonly IBrush LightTreeLeaveReference = new SolidColorBrush(Color.Parse("#795E26"));
        private static readonly IBrush LightParenthesis = new SolidColorBrush(Color.Parse("#5C2D91"));
        private static readonly IBrush LightPunctuation = new SolidColorBrush(Color.Parse("#666666"));
        private static readonly IBrush LightOperator = new SolidColorBrush(Color.Parse("#AF00DB"));
        private static readonly IBrush LightError = new SolidColorBrush(Color.Parse("#B00020"));

        // Тёмная тема (яркий текст на тёмном фоне, палитра в духе VS Code Dark+).
        private static readonly IBrush DarkDefault = new SolidColorBrush(Color.Parse("#D4D4D4"));
        private static readonly IBrush DarkFunction = new SolidColorBrush(Color.Parse("#4FC1FF"));
        private static readonly IBrush DarkIdentifier = new SolidColorBrush(Color.Parse("#9CDCFE"));
        private static readonly IBrush DarkNumber = new SolidColorBrush(Color.Parse("#B5CEA8"));
        private static readonly IBrush DarkString = new SolidColorBrush(Color.Parse("#CE9178"));
        private static readonly IBrush DarkTreeLeaveReference = new SolidColorBrush(Color.Parse("#DCDCAA"));
        private static readonly IBrush DarkParenthesis = new SolidColorBrush(Color.Parse("#C586C0"));
        private static readonly IBrush DarkPunctuation = new SolidColorBrush(Color.Parse("#BBBBBB"));
        private static readonly IBrush DarkOperator = new SolidColorBrush(Color.Parse("#D16EFF"));
        private static readonly IBrush DarkError = new SolidColorBrush(Color.Parse("#F48771"));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => GetBrush(value as FormulaHighlightKind? ?? FormulaHighlightKind.Default);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        /// <summary>Кисть текста сегмента по виду подсветки с учётом текущей темы.</summary>
        internal static IBrush GetBrush(FormulaHighlightKind kind)
        {
            var dark = IsDarkTheme();

            return kind switch
            {
                FormulaHighlightKind.Function => dark ? DarkFunction : LightFunction,
                FormulaHighlightKind.Identifier => dark ? DarkIdentifier : LightIdentifier,
                FormulaHighlightKind.Number => dark ? DarkNumber : LightNumber,
                FormulaHighlightKind.String => dark ? DarkString : LightString,
                FormulaHighlightKind.TreeLeaveReference => dark ? DarkTreeLeaveReference : LightTreeLeaveReference,
                FormulaHighlightKind.Parenthesis => dark ? DarkParenthesis : LightParenthesis,
                FormulaHighlightKind.Punctuation => dark ? DarkPunctuation : LightPunctuation,
                FormulaHighlightKind.Operator => dark ? DarkOperator : LightOperator,
                FormulaHighlightKind.Error => dark ? DarkError : LightError,
                _ => dark ? DarkDefault : LightDefault,
            };
        }

        internal static bool IsDarkTheme()
            => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    }

    /// <summary>
    /// Фон сегмента-скобки при подсветке парной скобки: полупрозрачный акцент,
    /// тема-зависимый (светлая — голубоватый, тёмная — синеватый), иначе прозрачный.
    /// </summary>
    public sealed class FormulaMatchingParenthesisToBrushConverter : IValueConverter
    {
        private static readonly IBrush LightHighlight = new SolidColorBrush(Color.Parse("#D7ECFF"));
        private static readonly IBrush DarkHighlight = new SolidColorBrush(Color.Parse("#3D5A80"));

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not true)
            {
                return Brushes.Transparent;
            }

            return FormulaHighlightKindToBrushConverter.IsDarkTheme() ? DarkHighlight : LightHighlight;
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
    /// Пустая часть (например "Moccasin|") → <see cref="AvaloniaProperty.UnsetValue"/>:
    /// свойство возвращается к значению по умолчанию/наследуемому из темы.
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

            if (string.IsNullOrWhiteSpace(colorText))
            {
                return AvaloniaProperty.UnsetValue;
            }

            return new SolidColorBrush(Color.Parse(colorText));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
