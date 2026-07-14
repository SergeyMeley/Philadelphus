using System.Globalization;

using global::Avalonia.Data.Converters;

using Philadelphus.Presentation.Converters.Logic;
using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    public sealed class IconNameConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var icon = IconResolver.Resolve(value);
            if (parameter is "LaunchTab" && icon == AppIcon.RepositoryLogo)
            {
                return "home";
            }

            return ToIconName(icon);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static string ToIconName(AppIcon icon)
            => icon switch
            {
                AppIcon.StatusOk => "status-ok",
                AppIcon.StatusInfo => "status-info",
                AppIcon.StatusWarning => "status-warning",
                AppIcon.StatusError => "status-error",
                AppIcon.StatusAlarm => "status-alarm",
                AppIcon.RepositoryLogo => "repository-logo",
                AppIcon.Shrub => "repository-logo",
                AppIcon.WorkingTree => "tree",
                AppIcon.TreeRoot => "root",
                AppIcon.TreeNode => "node",
                AppIcon.TreeLeaf => "leave",
                AppIcon.Attribute => "attribute",
                AppIcon.Add => "add",
                AppIcon.Open => "open",
                AppIcon.Storage => "storage",
                AppIcon.Settings => "settings",
                _ => "empty"
            };
    }
}
