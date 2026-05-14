using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для RepresentationToDataGridVisibilityConverter.
    /// </summary>
    public class RepresentationToDataGridVisibilityConverter : IValueConverter
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
            if ((PropertyGridRepresentations)value == PropertyGridRepresentations.DataGrid)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
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
