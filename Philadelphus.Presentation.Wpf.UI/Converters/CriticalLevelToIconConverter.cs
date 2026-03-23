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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

