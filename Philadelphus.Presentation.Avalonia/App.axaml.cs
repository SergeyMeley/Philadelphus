using System.Globalization;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Philadelphus.Presentation.Avalonia.Infrastructure;
using Philadelphus.Presentation.Avalonia.Infrastructure.Splash;
using Philadelphus.Presentation.Avalonia.Infrastructure.Startup;
using Philadelphus.Presentation.Avalonia.Infrastructure.Theme;
using Philadelphus.Presentation.Services.Interfaces;

using Serilog;

namespace Philadelphus.Presentation.Avalonia
{
    /// <summary>
    /// Точка композиции Avalonia-приложения. Повторяет bootstrap WPF App.xaml.cs (Host, Serilog,
    /// AutoMapper-скан, Kafka, Redis, конфиг-файлы, доменные сервисы, FormulaEngine, ImportExport),
    /// вызывает <c>AddPhiladelphusPresentation()</c> и добавляет платформенные (Avalonia) реализации,
    /// окна и реестр ViewModel→Window.
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            // Переопрос доступности команд по вводу (аналог WPF CommandManager.RequerySuggested).
            AvaloniaCommandManager.Initialize();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += OnExit;

                // Применяем сохранённую тему до показа splash — иначе сплэш рисуется раньше темы
                // и игнорирует выбранную светлую/тёмную схему.
                AppThemeBootstrapper.ApplySavedTheme(this);

                // Основной вариант — внешний splash-процесс: его анимация не зависит от UI-dispatcher этого процесса.
                // Если процесс не стартовал, остаёмся на встроенном окне как на безопасном fallback.
                var splash = SplashControllerFactory.Create();

                Dispatcher.UIThread.Post(
                    () => _ = InitializeAsync(desktop, splash),
                    DispatcherPriority.Background);
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task InitializeAsync(IClassicDesktopStyleApplicationLifetime desktop, ISplashController splash)
        {
            var startedAt = Environment.TickCount64;

            try
            {
                await Task.Delay(100).ConfigureAwait(true);
                splash.SetStatus("Загружаю конфигурацию...");

                // Подъём Host — в фоновом потоке: UI-поток свободен для анимации splash.
                await Task.Run(() =>
                {
                    splash.SetStatus("Собираю сервисы приложения...");
                    BuildHost();
                }).ConfigureAwait(true);

                splash.SetStatus("Запускаю сервисы приложения...");
                await Task.Yield();

                Log.Information("Запуск Host...");
                await Task.Run(() => _host!.StartAsync()).ConfigureAwait(true);

                splash.SetStatus("Применяю тему...");
                await Task.Yield();

                // Применяем сохранённую тему до показа окон (конструктор сервиса применяет режим).
                _ = _host!.Services.GetRequiredService<IThemeService>();

                splash.SetStatus("Регистрирую окна...");
                await Task.Yield();

                AvaloniaWindowRegistry.Register(_host!.Services);

                splash.SetStatus("Создаю стартовое окно...");
                await Task.Yield();

                // Стартовое окно — LaunchWindow.
                var launchWindow = AvaloniaWindowRegistry.CreateLaunchWindow(_host.Services);
                desktop.MainWindow = launchWindow;

                splash.SetStatus("Открываю приложение...");

                // Минимальное время показа splash, чтобы он не мелькал на быстром старте.
                const long MinSplashMs = 1200;
                var elapsed = Environment.TickCount64 - startedAt;
                if (elapsed < MinSplashMs)
                {
                    await Task.Delay((int)(MinSplashMs - elapsed)).ConfigureAwait(true);
                }

                launchWindow.Show();
                splash.Close();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка запуска приложения");
                splash.Close();
                desktop.Shutdown(-1);
            }
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Log.Information("Завершение приложения...");
            _host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
            Log.CloseAndFlush();
        }

        private void BuildHost()
        {
            _host = AvaloniaAppHostBuilder.Build();
        }
    }
}
