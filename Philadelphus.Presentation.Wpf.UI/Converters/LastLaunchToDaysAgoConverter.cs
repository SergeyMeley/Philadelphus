using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="LastLaunchToDaysAgoLogic" />.
    /// </summary>
    public class LastLaunchToDaysAgoConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует значение для Convert.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => LastLaunchToDaysAgoLogic.Convert(value);

        /// <summary>
        /// Преобразует значение для ConvertBack.
        /// </summary>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
