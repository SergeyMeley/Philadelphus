using System;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class IconToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string key)
            {
                // Пробуем найти в ресурсах текущего окна/приложения
                if (Application.Current.Resources[key] is Image image)
                    return image.Source; // или сам image

                // Fallback - пустая иконка
                return Application.Current.Resources["imageEmpty"] ?? null;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
