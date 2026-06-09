using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="UtcToLocalTimeLogic" />.
    /// </summary>
    public class UtcToLocalTimeConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует значение для Convert.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => UtcToLocalTimeLogic.Convert(value, parameter);

        /// <summary>
        /// Преобразует значение для ConvertBack.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => UtcToLocalTimeLogic.ConvertBack(value);
    }
}
