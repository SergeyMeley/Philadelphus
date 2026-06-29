using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter. Логика — в <see cref="EnumDisplayLogic" />.
    /// </summary>
    public sealed class EnumDisplayAttributeConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => EnumDisplayLogic.Convert(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
