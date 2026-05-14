using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для SelectedIndexConverter.
    /// </summary>
    public class SelectedIndexConverter : IValueConverter
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
            return ((int)value + 1).ToString();
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
