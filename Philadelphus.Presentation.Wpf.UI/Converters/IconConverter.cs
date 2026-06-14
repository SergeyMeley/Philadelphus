using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Wpf.UI.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// Единый конвертер иконок: значение → AppIcon (<see cref="IconResolver" />) → ImageSource (<see cref="AppIconImageSource" />).
    /// </summary>
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => AppIconImageSource.ToImageSource(IconResolver.Resolve(value));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
