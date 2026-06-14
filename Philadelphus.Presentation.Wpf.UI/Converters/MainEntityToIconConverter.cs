using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Wpf.UI.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика — <see cref="MainEntityToIconLogic" />, материализация — <see cref="AppIconImageSource" />.
    /// </summary>
    public class MainEntityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => AppIconImageSource.ToImageSource(MainEntityToIconLogic.ResolveIcon(value));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}