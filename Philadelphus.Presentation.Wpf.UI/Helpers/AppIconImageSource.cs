using Philadelphus.Presentation.Enums;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Philadelphus.Presentation.Wpf.UI.Helpers
{
    /// <summary>
    /// Материализация <see cref="AppIcon" /> в WPF <see cref="ImageSource" />.
    /// Файлы берутся из Assets/Icons рядом с приложением (site-of-origin).
    /// </summary>
    internal static class AppIconImageSource
    {
        private const string BaseUri = "pack://siteoforigin:,,,/Icons/";

        public static ImageSource ToImageSource(AppIcon icon)
        {
            var uri = new Uri(BaseUri + ResolveRelativePath(icon), UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private static string ResolveRelativePath(AppIcon icon)
            => icon switch
            {
                AppIcon.Empty => "without_a_license/Flaticon_icon_empty.png",
                AppIcon.StatusOk => "ok_64_1.png",
                AppIcon.StatusInfo => "info_64_2.png",
                AppIcon.StatusWarning => "warning_64.png",
                AppIcon.StatusError => "error_64_3.png",
                AppIcon.StatusAlarm => "alarm_64.png",
                AppIcon.RepositoryLogo => "philadelphus_logo_64.png",
                AppIcon.TreeRoot => "root_64_1.png",
                AppIcon.TreeNode => "node_64_3.png",
                AppIcon.TreeLeaf => "leave_64_3.png",
                AppIcon.Add => "without_a_license/Flaticon_icon_add.png",
                AppIcon.Open => "open.png",
                AppIcon.Storage => "storage.png",
                AppIcon.Settings => "settings_64.png",
                _ => "without_a_license/Flaticon_icon_empty.png"
            };
    }
}
