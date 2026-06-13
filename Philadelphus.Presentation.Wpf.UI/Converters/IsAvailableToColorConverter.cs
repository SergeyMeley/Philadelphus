using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="IsAvailableToColorLogic" />.
    /// </summary>
    public class IsAvailableToColorConverter : IValueConverter
    {
        private static readonly BrushConverter BrushConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => BrushConverter.ConvertFromString(IsAvailableToColorLogic.ResolveColorName(value))!;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
