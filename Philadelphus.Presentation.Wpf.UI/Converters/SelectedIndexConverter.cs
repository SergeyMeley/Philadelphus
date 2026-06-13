using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="SelectedIndexLogic" />.
    /// </summary>
    public class SelectedIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => SelectedIndexLogic.Convert(value);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
