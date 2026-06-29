using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Avalonia-обёртка IValueConverter. Логика — в <see cref="SelectedIndexLogic" />.
    /// </summary>
    public sealed class SelectedIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => SelectedIndexLogic.Convert(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
