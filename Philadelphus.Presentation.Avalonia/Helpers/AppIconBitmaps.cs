using System.Collections.Concurrent;

using global::Avalonia.Media.Imaging;
using global::Avalonia.Platform;

using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Avalonia.Helpers
{
    /// <summary>
    /// Материализация <see cref="AppIcon" /> в Avalonia <see cref="Bitmap" />.
    /// Файлы линкуются как AvaloniaResource и доступны по схеме avares://.
    /// Аналог WPF-провайдера AppIconImageSource (там — ImageSource через site-of-origin).
    /// </summary>
    internal static class AppIconBitmaps
    {
        private const string BaseUri = "avares://Philadelphus.Presentation.Avalonia/Assets/Icons/";

        private static readonly ConcurrentDictionary<AppIcon, Bitmap> Cache = new();

        /// <summary>
        /// Возвращает кэшированный <see cref="Bitmap" /> для указанной иконки.
        /// </summary>
        public static Bitmap ToBitmap(AppIcon icon)
            => Cache.GetOrAdd(icon, Load);

        private static Bitmap Load(AppIcon icon)
        {
            var uri = new Uri(BaseUri + ResolveRelativePath(icon), UriKind.Absolute);
            using var stream = AssetLoader.Open(uri);
            return new Bitmap(stream);
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
