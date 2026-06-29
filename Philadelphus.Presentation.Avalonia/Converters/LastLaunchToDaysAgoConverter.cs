using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter. Логика — в <see cref="LastLaunchToDaysAgoLogic" />.
    /// </summary>
    public sealed class LastLaunchToDaysAgoConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => LastLaunchToDaysAgoLogic.Convert(value);

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
