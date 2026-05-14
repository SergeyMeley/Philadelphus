using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    class CriticalLevelToIconConverter : IValueConverter
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
            if (value is NotificationCriticalLevelModel cl)
            {
                string key = "imageEmpty";
                switch (cl)
                {
                    case NotificationCriticalLevelModel.None:
                        key = "imageEmpty";
                        break;
                    case NotificationCriticalLevelModel.Ok:
                        key = "imageOk64_1";
                        break;
                    case NotificationCriticalLevelModel.Info:
                        key = "imageInfo64_2";
                        break;
                    case NotificationCriticalLevelModel.Warning:
                        key = "imageWarning64";
                        break;
                    case NotificationCriticalLevelModel.Error:
                        key = "imageError64_3";
                        break;
                    case NotificationCriticalLevelModel.Alarm:
                        key = "imageAlarm64";
                        break;
                    default:
                        break;
                }

                if (Application.Current.Resources[key] is Image image)
                    return image.Source;

                return Application.Current.Resources[key] ?? null;
            }
            return null;
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
            => throw new NotImplementedException();
    }
}

