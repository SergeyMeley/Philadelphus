using System.IO;

using global::Avalonia;
using global::Avalonia.Styling;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Theme
{
    internal static class AppThemeBootstrapper
    {
        public static void ApplySavedTheme(Application application)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(path) == false)
                {
                    application.RequestedThemeVariant = ThemeVariant.Default;
                    return;
                }

                var root = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path)) as System.Text.Json.Nodes.JsonObject;
                var themeString = root?["AppearanceConfig"]?["ThemeString"]?.GetValue<string>();

                application.RequestedThemeVariant = themeString?.Trim().ToLowerInvariant() switch
                {
                    "light" => ThemeVariant.Light,
                    "dark" => ThemeVariant.Dark,
                    _ => ThemeVariant.Default,
                };
            }
            catch
            {
                application.RequestedThemeVariant = ThemeVariant.Default;
            }
        }
    }
}
