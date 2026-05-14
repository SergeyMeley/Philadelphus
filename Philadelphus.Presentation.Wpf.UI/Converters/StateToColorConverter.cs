using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для StateToColorConverter.
    /// </summary>
    public class StateToColorConverter : IValueConverter
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
            if (value is State state)
            {
                switch (state)
                {
                    case State.Initialized:
                        return Brushes.DeepPink;
                        break;
                    case State.Changed:
                        return Brushes.Cyan;
                        break;
                    case State.SavedOrLoaded:
                        return Brushes.YellowGreen;
                        break;
                    case State.ForSoftDelete:
                        return Brushes.OrangeRed;
                        break;
                    case State.ForHardDelete:
                        return Brushes.Red;
                        break;
                    case State.SoftDeleted:
                        return Brushes.IndianRed;
                        break;
                    default:
                        break;
                }
            }
            return Brushes.White;
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
