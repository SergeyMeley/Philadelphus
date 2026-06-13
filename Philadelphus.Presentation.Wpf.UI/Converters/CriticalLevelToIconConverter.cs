using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="CriticalLevelToIconLogic" />.
    /// </summary>
    class CriticalLevelToIconConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = CriticalLevelToIconLogic.ResolveResourceKey(value);
            if (key is null)
                return null;

            if (Application.Current.Resources[key] is Image image)
                return image.Source;

            return Application.Current.Resources[key];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}