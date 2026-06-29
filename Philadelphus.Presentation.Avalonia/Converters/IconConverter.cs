using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Converters.Logic;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Единый конвертер иконок: значение → AppIcon (<see cref="IconResolver" />) → Bitmap (<see cref="AppIconBitmaps" />).
    /// </summary>
    public sealed class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => AppIconBitmaps.ToBitmap(IconResolver.Resolve(value));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
