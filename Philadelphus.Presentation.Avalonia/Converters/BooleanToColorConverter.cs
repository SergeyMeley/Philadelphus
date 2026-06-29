using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter для bool/null → кисть.
    /// Логика — в <see cref="BooleanToColorLogic" />, материализация — в <see cref="ConverterColorBrushes" />.
    /// </summary>
    public sealed class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ConverterColorBrushes.ToBrush(BooleanToColorLogic.ResolveColor(value));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
