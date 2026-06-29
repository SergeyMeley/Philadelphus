using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter. Логика — в <see cref="UtcToLocalTimeLogic" />.
    /// </summary>
    public sealed class UtcToLocalTimeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => UtcToLocalTimeLogic.Convert(value, parameter);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => UtcToLocalTimeLogic.ConvertBack(value);
    }
}
