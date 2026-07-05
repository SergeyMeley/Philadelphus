using global::Avalonia;

using Philadelphus.Presentation.Avalonia.Infrastructure.Splash;

namespace Philadelphus.Presentation.Avalonia
{
    internal static class Program
    {
        // Точка входа. Не использовать Avalonia/сторонние API до AppMain (см. рекомендации Avalonia).
        [STAThread]
        public static void Main(string[] args)
        {
            if (SplashProcessHost.TryInitialize(args))
            {
                BuildSplashAvaloniaApp().StartWithClassicDesktopLifetime(args);
                return;
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Используется визуальным дизайнером Avalonia.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();

        private static AppBuilder BuildSplashAvaloniaApp()
            => AppBuilder.Configure<SplashOnlyApp>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
