using System.Diagnostics;
using System.IO;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Themes.Fluent;
using global::Avalonia.Threading;

using Philadelphus.Presentation.Avalonia.Views.Windows;

namespace Philadelphus.Presentation.Avalonia
{
    internal sealed class SplashOnlyApp : Application
    {
        private DispatcherTimer? _controlTimer;
        private DateTime _lastStatusWriteTimeUtc;

        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
            ApplySavedThemeEarly();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                ApplySplashThemeResources();

                var splash = new SplashWindow();
                desktop.MainWindow = splash;
                splash.Show();

                _controlTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(200),
                };
                _controlTimer.Tick += (_, _) => UpdateSplashProcess(desktop, splash);
                _controlTimer.Start();
                UpdateSplashStatus(splash);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ApplySavedThemeEarly()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(path) == false)
                {
                    RequestedThemeVariant = ThemeVariant.Default;
                    return;
                }

                var root = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(path)) as System.Text.Json.Nodes.JsonObject;
                var themeString = root?["AppearanceConfig"]?["ThemeString"]?.GetValue<string>();

                RequestedThemeVariant = themeString?.Trim().ToLowerInvariant() switch
                {
                    "light" => ThemeVariant.Light,
                    "dark" => ThemeVariant.Dark,
                    _ => ThemeVariant.Default,
                };
            }
            catch
            {
                RequestedThemeVariant = ThemeVariant.Default;
            }
        }

        private void ApplySplashThemeResources()
        {
            var isDark = ActualThemeVariant == ThemeVariant.Dark;

            Resources["SystemControlBackgroundAltHighBrush"] = new SolidColorBrush(
                Color.Parse(isDark ? "#FF252526" : "#FFFFFFFF"));
            Resources["SystemControlForegroundBaseLowBrush"] = new SolidColorBrush(
                Color.Parse(isDark ? "#66FFFFFF" : "#33000000"));
            Resources["SystemControlForegroundBaseMediumLowBrush"] = new SolidColorBrush(
                Color.Parse(isDark ? "#99FFFFFF" : "#99000000"));
            Resources["SystemControlForegroundBaseMediumBrush"] = new SolidColorBrush(
                Color.Parse(isDark ? "#CCFFFFFF" : "#CC000000"));
        }

        private void UpdateSplashProcess(IClassicDesktopStyleApplicationLifetime desktop, SplashWindow splash)
        {
            if (SplashProcessHost.ShouldClose || SplashProcessHost.IsParentAlive() == false)
            {
                _controlTimer?.Stop();
                splash.Close();
                desktop.Shutdown();
                SplashProcessHost.TryCleanup();
                return;
            }

            UpdateSplashStatus(splash);
        }

        private void UpdateSplashStatus(SplashWindow splash)
        {
            try
            {
                var statusFilePath = SplashProcessHost.StatusFilePath;
                if (string.IsNullOrWhiteSpace(statusFilePath) || File.Exists(statusFilePath) == false)
                {
                    return;
                }

                var writeTimeUtc = File.GetLastWriteTimeUtc(statusFilePath);
                if (writeTimeUtc == _lastStatusWriteTimeUtc)
                {
                    return;
                }

                _lastStatusWriteTimeUtc = writeTimeUtc;
                var status = File.ReadAllText(statusFilePath).Trim();
                if (string.IsNullOrWhiteSpace(status) == false)
                {
                    splash.SetStatus(status);
                }
            }
            catch
            {
                // Статус будет перечитан на следующем тике таймера.
            }
        }
    }

    internal static class SplashProcessHost
    {
        public const string Argument = "--philadelphus-splash";

        private static string? _closeSignalPath;
        private static int _parentProcessId;

        public static string? StatusFilePath { get; private set; }

        public static bool ShouldClose
            => string.IsNullOrWhiteSpace(_closeSignalPath) == false && File.Exists(_closeSignalPath);

        public static bool TryInitialize(string[] args)
        {
            if (args.Length < 4 || args[0] != Argument)
            {
                return false;
            }

            StatusFilePath = args[1];
            _closeSignalPath = args[2];
            _ = int.TryParse(args[3], out _parentProcessId);
            return true;
        }

        public static bool IsParentAlive()
        {
            if (_parentProcessId <= 0)
            {
                return true;
            }

            try
            {
                var parentProcess = Process.GetProcessById(_parentProcessId);
                return parentProcess.HasExited == false;
            }
            catch
            {
                return false;
            }
        }

        public static void TryCleanup()
        {
            TryDeleteFile(StatusFilePath);
            TryDeleteFile(_closeSignalPath);

            var directoryPath = Path.GetDirectoryName(StatusFilePath);
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return;
            }

            try
            {
                if (Directory.Exists(directoryPath) && Directory.GetFiles(directoryPath).Length == 0)
                {
                    Directory.Delete(directoryPath);
                }
            }
            catch
            {
                // Временные файлы splash не критичны для работы приложения.
            }
        }

        private static void TryDeleteFile(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) == false && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Временные файлы splash не критичны для работы приложения.
            }
        }
    }
}
