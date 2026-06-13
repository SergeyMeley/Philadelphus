using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="StateToColorLogic" />.
    /// </summary>
    public class StateToColorConverter : IValueConverter
    {
        private static readonly BrushConverter BrushConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => BrushConverter.ConvertFromString(StateToColorLogic.ResolveColorName(value))!;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
