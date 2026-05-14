using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для IsAvailableToColorConverter.
    /// </summary>
    public class IsAvailableToColorConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует значение для Convert.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="targetType">Целевой тип преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Преобразованное значение.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result = Brushes.DarkRed;
            if (value == null)
                result = Brushes.Black;
            if (value is bool)
            {
                if ((bool)value)
                    result = Brushes.Green;
                else
                    result = Brushes.Red;
            }
            return result;
        }

        /// <summary>
        /// Преобразует значение для ConvertBack.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="targetType">Целевой тип преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Преобразованное значение.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
