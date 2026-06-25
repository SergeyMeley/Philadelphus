using global::Avalonia;

namespace Philadelphus.Presentation.Avalonia
{
    internal static class Program
    {
        // Точка входа. Не использовать Avalonia/сторонние API до AppMain (см. рекомендации Avalonia).
        [STAThread]
        public static void Main(string[] args)
            => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Используется визуальным дизайнером Avalonia.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
