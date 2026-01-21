using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Factories.Implementations;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IHost _host;
        private readonly IConfiguration _configuration;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public App()
        {
            AllocConsole();
            Console.Title = "Philadelphus Startup Log";

            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            // 1. Startup Logger: Console + File
            var startupConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(startupConfig)  // Читает "Serilog" секцию
                .MinimumLevel.Information()
                .CreateLogger();

            Log.Information("=== Philadelphus Startup: Console + File Logging ===");

            // Создание временной конфигурации для Host
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                // Добавление конфигурационных файлов
                .ConfigureAppConfiguration(async (hostingContext, config) =>
                {
                    // Основной конфигурационный файл
                    config.AddConfiguration(_configuration);  // Временная конфигурация
                    
                    // Дополнительные конфигурационные файлы
                    var additionalConfigsPath = Environment.ExpandEnvironmentVariables(_configuration["AdditionalConfigs:BasePath"] ?? "%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");

                    Log.Information($"Проверка базовой директории: {additionalConfigsPath}");

                    if (Directory.Exists(additionalConfigsPath) == false)
                    {
                        Log.Warning($"Директория не существует: {additionalConfigsPath}. Создаётся...");
                        try
                        {
                            Directory.CreateDirectory(additionalConfigsPath);
                            Log.Information($"Директория создана: {additionalConfigsPath}");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Ошибка создания директории {additionalConfigsPath}");
                        }
                    }

                    // Фиксированный список конфигов
                    var configFiles = new Dictionary<string, Func<object>>
                    {
                        ["storages-config.json"] = () => new DataStoragesCollection { DataStorages = new() },
                        ["repository-headers-config.json"] = () => new TreeRepositoryHeadersCollection { TreeRepositoryHeaders = new() }
                    };

                    foreach (var kvp in configFiles)
                    {
                        var fullPath = Path.Combine(additionalConfigsPath, kvp.Key);
                        Log.Information($"Проверка конфига: {additionalConfigsPath}", fullPath);

                        if (File.Exists(fullPath) == false)
                        {
                            Log.Warning($"Не найден {additionalConfigsPath}, создаётся default", fullPath);
                            var emptyConfig = kvp.Value();
                            await InitEmptyConfigAsync(emptyConfig, fullPath);
                            Log.Information($"Создан default: {additionalConfigsPath}", fullPath);
                        }
                        else
                        {
                            Log.Debug($"Найден и будет загружен: {additionalConfigsPath}", fullPath);
                        }

                        config.AddJsonFile(fullPath, optional: true, reloadOnChange: true);
                    }


                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment() || true /*временно для тестов*/)
                    {
                        var appAssembly = Assembly.GetExecutingAssembly();
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    // Регистрация конфигурации
                    services.Configure<ApplicationSettings>(
                        context.Configuration.GetSection(nameof(ApplicationSettings)));
                    services.Configure<ConnectionStringsCollection>(
                        context.Configuration.GetSection(nameof(ConnectionStringsCollection)));
                    services.Configure<DataStoragesCollection>(
                        context.Configuration.GetSection(nameof(DataStoragesCollection)));
                    services.Configure<TreeRepositoryHeadersCollection>(
                        context.Configuration.GetSection(nameof(TreeRepositoryHeadersCollection)));

                    // Регистрация AutoMapper
                    services.AddAutoMapper(typeof(MappingProfile));
                    
                    // TODO: Проработать кеширование
                    services.AddMemoryCache();

                    // Регистрация сервисов
                    //services.AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();     Заменено на IOptions<T>
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IDataStoragesService, DataStoragesService>();
                    services.AddTransient<ITreeRepositoryCollectionService, TreeRepositoryCollectionService>();
                    services.AddTransient<ITreeRepositoryService, TreeRepositoryService>();
                    services.AddTransient<IExtensionManager, ExtensionManager>();

                    // Регистрация ViewModel
                    services.AddSingleton<ApplicationVM>();
                    services.AddSingleton<ApplicationCommandsVM>();
                    services.AddTransient<ApplicationWindowsVM>();
                    services.AddTransient<LaunchWindowVM>();
                    //services.AddTransient<MainWindowVM>();                    // Заменено на фабрику
                    services.AddSingleton<DataStoragesSettingsVM>();
                    //services.AddTransient<RepositoryExplorerControlVM>();     // Заменено на фабрику
                    services.AddTransient<TreeRepositoryCollectionVM>();
                    services.AddTransient<TreeRepositoryHeadersCollectionVM>();
                    services.AddTransient<RepositoryCreationControlVM>();

                    // Регистрация View
                    services.AddTransient<MainWindow>();
                    services.AddTransient<LaunchWindow>();

                    // Регистрация фабрик
                    services.AddTransient<IMainWindowVMFactory, MainWindowVMFactory>();
                    services.AddTransient<IRepositoryExplorerControlVMFactory, RepositoryExplorerControlVMFactory>();
                    services.AddTransient<IExtensionsControlVMFactory, ExtensionsControlVMFactory>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                Log.Information("Запуск Host...");
                await _host.StartAsync();

                // 2. Переконфигурация: только File (закрыть Console)
                Log.Information("Startup завершён. Переключение на File-only logging...");

                await Task.Delay(3000);

                var runtimeLogger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_configuration)
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Fatal)  // Только Fatal в Console
                    .MinimumLevel.Information()
                    .CreateLogger();

                Log.Logger = runtimeLogger;
                FreeConsole();  // Закрыть консольное окно

                Log.Information("UI запущен. Логи → только файл logs/philadelphus-.log");

                var window = _host.Services.GetRequiredService<LaunchWindow>();
                window.Topmost = true;
                window.Show();
                window.Activate();
                window.Topmost = false;
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Критическая ошибка запуска приложения");
                MessageBox.Show($"Ошибка запуска: {ex.Message}");
                Shutdown(-1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            Log.Information("Завершение приложения...");
            await _host.StopAsync();
            _host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        private async Task InitEmptyConfigAsync<T>(T configObject, string path)
        {
            var json = JsonSerializer.Serialize(configObject, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
        }
    }
}
