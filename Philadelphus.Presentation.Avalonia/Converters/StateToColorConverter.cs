using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter. Логика — в <see cref="StateToColorLogic" />.
    /// </summary>
    public sealed class StateToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => ConverterColorBrushes.ToBrush(StateToColorLogic.ResolveColor(value));

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
