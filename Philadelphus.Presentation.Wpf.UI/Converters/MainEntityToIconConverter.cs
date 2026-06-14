using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter. Логика вынесена в <see cref="MainEntityToIconLogic" />.
    /// </summary>
    public class MainEntityToIconConverter : IValueConverter
    {
        private static readonly string BaseUri = "pack://siteoforigin:,,,/Icons/";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = new Uri(BaseUri + MainEntityToIconLogic.ResolveIconFileName(value), UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}