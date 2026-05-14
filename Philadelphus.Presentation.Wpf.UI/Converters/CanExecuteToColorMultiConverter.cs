using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для CanExecuteToColorMultiConverter.
    /// </summary>
    public class CanExecuteToColorMultiConverter : IMultiValueConverter
    {
        /// <summary>
        /// Преобразует значение для Convert.
        /// </summary>
        /// <param name="values">Значения для преобразования.</param>
        /// <param name="targetType">Целевой тип преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Преобразованное значение.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0 && values[0] is bool canExecute)
            {
                return canExecute ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            }
            return System.Windows.Media.Brushes.Gray;
        }

        /// <summary>
        /// Преобразует значение для ConvertBack.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="targetTypes">Целевые типы преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Коллекция полученных данных.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
