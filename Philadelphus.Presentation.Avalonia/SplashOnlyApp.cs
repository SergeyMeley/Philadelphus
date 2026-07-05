using System.IO;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Media;
using global::Avalonia.Styling;
using global::Avalonia.Themes.Fluent;
using global::Avalonia.Threading;

using Philadelphus.Presentation.Avalonia.Infrastructure.Splash;
using Philadelphus.Presentation.Avalonia.Infrastructure.Theme;
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
            AppThemeBootstrapper.ApplySavedTheme(this);
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

}
