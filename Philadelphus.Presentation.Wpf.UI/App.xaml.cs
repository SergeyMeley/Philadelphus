using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Factories.Implementations;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Mapping;
using Philadelphus.Presentation.Wpf.UI.Services.Implementations;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Shapes;

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

            Log.Information($"Загружаю конфиг из: {Directory.GetCurrentDirectory()}");

            _configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                // Добавление конфигурационных файлов
                .ConfigureAppConfiguration(async (hostingContext, config) =>
                {
                    // Основной конфигурационный файл
                    config.AddConfiguration(_configuration);  // Временная конфигурация
                    
                    // Дополнительные конфигурационные файлы
                    var basePath = Environment.ExpandEnvironmentVariables(_configuration["ApplicationSettings:BasePath"] ?? "%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");

                    Log.Information($"Проверка базовой директории: {basePath}");

                    await ConfigurationService.CheckOrInitDirectory(new DirectoryInfo(basePath));

                    // Фиксированный список конфигов
                     var configFiles = new Dictionary<string, object>
                    {
                        [Environment.ExpandEnvironmentVariables(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(ConnectionStringsCollectionConfig)}"])]
                            = new ConnectionStringsCollectionConfig { ConnectionStringContainers = new() },
                         [Environment.ExpandEnvironmentVariables(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(DataStoragesCollectionConfig)}"])]
                            = new DataStoragesCollectionConfig { DataStorages = new() },
                         [Environment.ExpandEnvironmentVariables(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(TreeRepositoryHeadersCollectionConfig)}"])]
                            = new TreeRepositoryHeadersCollectionConfig { TreeRepositoryHeaders = new() }
                     };

                    foreach (var kvp in configFiles)
                    {
                        var file = new FileInfo(kvp.Key);
                        await ConfigurationService.CheckOrInitDirectory(file.Directory);
                        await ConfigurationService.CheckOrInitFile(file, kvp.Value);
                        config.AddJsonFile(file.FullName, optional: true, reloadOnChange: true);
                    }

                    Console.WriteLine("=== ДИАГНОСТИКА СРЕДЫ ===");
                    Console.WriteLine($"DOTNET_ENVIRONMENT: '{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}'");
                    Console.WriteLine($"ASPNETCORE_ENVIRONMENT: '{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}'");
                    Console.WriteLine($"DOTNET_ENVIRONMENT (все): {Environment.GetEnvironmentVariables()["DOTNET_ENVIRONMENT"]}");
                    Console.WriteLine($"Launch Profile: {Environment.GetEnvironmentVariable("DOTNET_LAUNCH_PROFILE")}");
                    Console.WriteLine("=========================");

                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.GetExecutingAssembly();
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    // Регистрация конфигурации
                    services.Configure<ApplicationSettingsConfig>(
                        context.Configuration.GetSection(nameof(ApplicationSettingsConfig)));
                    services.Configure<ConnectionStringsCollectionConfig>(
                        context.Configuration.GetSection(nameof(ConnectionStringsCollectionConfig)));
                    services.Configure<DataStoragesCollectionConfig>(
                        context.Configuration.GetSection(nameof(DataStoragesCollectionConfig)));
                    services.Configure<TreeRepositoryHeadersCollectionConfig>(
                        context.Configuration.GetSection(nameof(TreeRepositoryHeadersCollectionConfig)));

                    // Регистрация AutoMapper
                    services.AddAutoMapper(
                        typeof(DomainMappingProfile),       // Model <-> Db Entity
                        typeof(ViewModelsMappingProfile)    // Model <-> ViewModel
                        );
                    

                    services.AddStackExchangeRedisCache(options =>
                    {
                        var settings = services.BuildServiceProvider()
                            .GetRequiredService<IOptions<ApplicationSettingsConfig>>().Value;

                        options.Configuration = settings.RedisOptions.Socket +
                            (string.IsNullOrEmpty(settings.RedisOptions.Password)
                                ? "" : $",password={settings.RedisOptions.Password}");
                        options.InstanceName = "Philadelphus:";

                        options.Configuration = "localhost:6379,password=philapass";
                    });

                    // Регистрация сервисов
                    // Слой Core
                    //services.AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();     Заменено на IOptions<T>
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IDataStoragesService, DataStoragesService>();
                    services.AddTransient<ITreeRepositoryCollectionService, TreeRepositoryCollectionService>();
                    services.AddTransient<ITreeRepositoryService, TreeRepositoryService>();
                    services.AddTransient<IExtensionManager, ExtensionManager>();
                    // Слой Presentation
                    services.AddSingleton<IConfigurationService, ConfigurationService>();

                    // Регистрация ViewModel
                    services.AddSingleton<ApplicationVM>();
                    services.AddSingleton<ApplicationSettingsControlVM>();
                    services.AddSingleton<ApplicationCommandsVM>();
                    services.AddTransient<ApplicationWindowsVM>();
                    services.AddTransient<LaunchWindowVM>();
                    //services.AddTransient<MainWindowVM>();                    // Заменено на фабрику
                    services.AddSingleton<DataStoragesCollectionVM>();
                    //services.AddTransient<RepositoryExplorerControlVM>();     // Заменено на фабрику
                    services.AddTransient<TreeRepositoryCollectionVM>();
                    services.AddTransient<TreeRepositoryHeadersCollectionVM>();
                    services.AddTransient<StorageCreationControlVM>();
                    services.AddTransient<RepositoryCreationControlVM>();
                    services.AddTransient<LaunchWindowTabItemControlVM>();
                    services.AddTransient<ApplicationSettingsTabItemControlVM>();

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

                Log.Information("Искусственная задержка запуска 2 сек.");
                await Task.Delay(2000);


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
                MessageBox.Show($"Ошибка запуска:\r\n{ex.Message}\r\nПодробности:{ex.StackTrace}");
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

    }
}
