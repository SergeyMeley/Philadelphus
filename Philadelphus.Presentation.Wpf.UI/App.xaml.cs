using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Messaging.Kafka;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Factories.Implementations;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Mapping;
using Philadelphus.Presentation.Wpf.UI.Services.Implementations;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Configuration;
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

            var runtimeLogger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File("logs/philadelphus-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Logger = runtimeLogger;

            //Log.Logger = new LoggerConfiguration()
            //    .ReadFrom.Configuration(startupConfig)  // Читает "Serilog" секцию
            //    .MinimumLevel.Information()
            //    .CreateLogger();

            Log.Information("=== Philadelphus Startup: Console + File Logging ===");

            // Создание временной конфигурации для Host
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Log.Information($"Загружаю конфиг из: {Directory.GetCurrentDirectory()}");

            _configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                //.UseSerilog()
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
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
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(PhiladelphusRepositoryHeadersCollectionConfig)}"])]
                            = new PhiladelphusRepositoryHeadersCollectionConfig { PhiladelphusRepositoryHeaders = new() }
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
                    services.AddLogging(builder => builder.AddSerilog());

                    // Регистрация конфигурации
                    services.Configure<ApplicationSettingsConfig>(
                        context.Configuration.GetSection(nameof(ApplicationSettingsConfig)));
                    services.Configure<ConnectionStringsCollectionConfig>(
                        context.Configuration.GetSection(nameof(ConnectionStringsCollectionConfig)));
                    services.Configure<DataStoragesCollectionConfig>(
                        context.Configuration.GetSection(nameof(DataStoragesCollectionConfig)));
                    services.Configure<PhiladelphusRepositoryHeadersCollectionConfig>(
                        context.Configuration.GetSection(nameof(PhiladelphusRepositoryHeadersCollectionConfig)));
                    services.Configure<MessagingConfig>(
                        context.Configuration.GetSection(nameof(MessagingConfig)));

                    // Регистрация AutoMapper
                    var profileAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.GetTypes()
                            .Any(t => typeof(Profile).IsAssignableFrom(t) &&
                                     t.Namespace?.StartsWith("Philadelphus.") == true))
                        .ToArray();

                    services.AddAutoMapper(cfg => { }, profileAssemblies);

                    // Добавление отправителей и получателей сообщений
                    services.AddKafkaProducer<MessagingUser>(context.Configuration.GetSection($"{nameof(KafkaOptions<MessagingUser>)}:{nameof(MessagingUser)}"));
                    services.AddKafkaConsumer<MessagingUser>(context.Configuration.GetSection($"{nameof(KafkaOptions<MessagingUser>)}:{nameof(MessagingUser)}"));
                    services.AddKafkaProducer<Notification>(context.Configuration.GetSection($"{nameof(KafkaOptions<Notification>)}:{nameof(Notification)}"));
                    services.AddKafkaConsumer<Notification>(context.Configuration.GetSection($"{nameof(KafkaOptions<Notification>)}:{nameof(Notification)}"));


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
                    services.AddTransient<IPhiladelphusRepositoryCollectionService, PhiladelphusRepositoryCollectionService>();
                    services.AddTransient<IPhiladelphusRepositoryService, PhiladelphusRepositoryService>();
                    services.AddTransient<IExtensionManager, ExtensionManager>();
                    // Слой Presentation
                    services.AddSingleton<IConfigurationService, ConfigurationService>();

                    // Регистрация ViewModel
                    // Общие ViewModel
                    services.AddSingleton<ApplicationVM>();
                    services.AddSingleton<ApplicationCommandsVM>();
                    services.AddSingleton<MainWindowNotificationsVM>();
                    // ViewModel окон
                    services.AddTransient<ApplicationWindowsVM>();
                    services.AddTransient<LaunchWindowVM>();
                    // ViewModel контролов
                    services.AddTransient<MessageLogControlVM>();
                    services.AddTransient<PopUpNotificationsControlVM>();
                    services.AddSingleton<ApplicationSettingsControlVM>();
                    services.AddTransient<ApplicationSettingsTabItemControlVM>();
                    services.AddTransient<LaunchWindowTabItemControlVM>();
                    services.AddTransient<StorageCreationControlVM>();
                    services.AddTransient<RepositoryCreationControlVM>();
                    //services.AddTransient<MainWindowVM>();                    // Заменено на фабрику
                    //services.AddTransient<RepositoryExplorerControlVM>();     // Заменено на фабрику
                    // ViewModel сущностей
                    services.AddSingleton<DataStoragesCollectionVM>();
                    services.AddSingleton<PhiladelphusRepositoryCollectionVM>();        // Не менять. Приводит к ошибкам обновления интерфейса
                    services.AddSingleton<PhiladelphusRepositoryHeadersCollectionVM>(); // Не менять. Приводит к ошибкам обновления интерфейса

                    // Регистрация View
                    services.AddTransient<MainWindow>();
                    services.AddSingleton<LaunchWindow>();      // Не менять. Требуется для автоматического закрытия окна при открытии основного
                    services.AddTransient<AttributeValuesCollectionWindow>();
                    services.AddSingleton<SplashWindow>();
                    services.AddTransient<ImportFromExcelWindow>();

                    // Регистрация фабрик
                    services.AddTransient<IMainWindowVMFactory, MainWindowVMFactory>();
                    services.AddTransient<IRepositoryExplorerControlVMFactory, RepositoryExplorerControlVMFactory>();
                    services.AddTransient<IExtensionsControlVMFactory, ExtensionsControlVMFactory>();
                    services.AddTransient<IInfrastructureRepositoryFactory, InfrastructureRepositoryFactory>();
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

                //var window = _host.Services.GetRequiredService<LaunchWindow>();
                //window.Topmost = true;
                //window.Show();
                //window.Activate();
                //window.Topmost = false;

                var window = _host.Services.GetRequiredService<SplashWindow>();
                window.Show();

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
