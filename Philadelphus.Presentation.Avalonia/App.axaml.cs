using System.Globalization;
using System.IO;
using System.Reflection;

using AutoMapper;

using global::Avalonia;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Markup.Xaml;
using global::Avalonia.Threading;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Identity.Configurations;
using Philadelphus.Core.Domain.Identity.Services.Implementations;
using Philadelphus.Core.Domain.Identity.Services.Interfaces;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Diagnostics;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Services.Implementations;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Factories;
using Philadelphus.Infrastructure.Cache.Redis.Implementations;
using Philadelphus.Infrastructure.Cache.RepositoryInterfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Infrastructure.ImportExport.Phjson;
using Philadelphus.Infrastructure.Messaging.Kafka;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Avalonia.Factories;
using Philadelphus.Presentation.Avalonia.Infrastructure;
using Philadelphus.Presentation.Avalonia.Infrastructure.Splash;
using Philadelphus.Presentation.Avalonia.Infrastructure.Theme;
using Philadelphus.Presentation.Avalonia.Services;
using Philadelphus.Presentation.Avalonia.Views.Windows;
using Philadelphus.Presentation.Configurations;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services;
using Philadelphus.Presentation.Services.Implementations;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.ImportExport;

using Serilog;

using StackExchange.Redis;

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
        private IConfiguration? _configuration;

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

                var windowService = _host!.Services.GetRequiredService<AvaloniaWindowService>();
                windowService.Register<MainWindowVM, MainWindow>();
                windowService.Register<LaunchWindowVM, LaunchWindow>();
                windowService.Register<FormulaTestControlVM, FormulaEditorWindow>();
                windowService.Register<RepositoryExplorerControlVM, AttributeValuesCollectionWindow>();
                windowService.Register<DetailsWindowVM, DetailsWindow>();
                windowService.Register<AboutWindowVM, AboutWindow>();
                windowService.Register<ExcelImportDesignerVM, ExcelImportDesignerWindow>();

                splash.SetStatus("Создаю стартовое окно...");
                await Task.Yield();

                // Стартовое окно — LaunchWindow.
                var launchWindow = _host.Services.GetRequiredService<LaunchWindow>();
                launchWindow.DataContext = _host.Services.GetRequiredService<LaunchWindowVM>();
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
            // Стартовый логгер: Console + File
            var startupConfig = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(startupConfig)
                .MinimumLevel.Information()
                .CreateLogger();

            Log.Information("=== Philadelphus Startup (Avalonia) ===");

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Log.Information($"Загружаю конфиг из: {AppContext.BaseDirectory}");

            _configuration = builder.Build();

            _host = Host.CreateDefaultBuilder()
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(_configuration);

                    var basePath = ConfigPathHelper.Expand(
                        _configuration["ApplicationSettings:BasePath"]
                        ?? "%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");

                    Log.Information($"Проверка базовой директории: {basePath}");

                    ConfigurationService.CheckOrInitDirectory(new DirectoryInfo(basePath)).GetAwaiter().GetResult();

                    var configFiles = new Dictionary<string, object>
                    {
                        [ConfigPathHelper.Expand(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(ConnectionStringsCollectionConfig)}"]!)]
                            = new ConnectionStringsCollectionConfig { ConnectionStringsContainers = new() },
                        [ConfigPathHelper.Expand(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(DataStoragesCollectionConfig)}"]!)]
                            = new DataStoragesCollectionConfig { DataStorages = new() },
                        [ConfigPathHelper.Expand(
                            _configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(PhiladelphusRepositoryHeadersCollectionConfig)}"]!)]
                            = new PhiladelphusRepositoryHeadersCollectionConfig { PhiladelphusRepositoryHeaders = new() }
                    };

                    foreach (var kvp in configFiles)
                    {
                        var file = new FileInfo(kvp.Key);
                        ConfigurationService.CheckOrInitDirectory(file.Directory!).GetAwaiter().GetResult();
                        ConfigurationService.CheckOrInitFile(file, kvp.Value).GetAwaiter().GetResult();
                        config.AddJsonFile(file.FullName, optional: true, reloadOnChange: true);
                    }

                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.GetExecutingAssembly();
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

                    // Конфигурация
                    services.Configure<ApplicationSettingsConfig>(
                        context.Configuration.GetSection(nameof(ApplicationSettingsConfig)));
                    services.Configure<AppearanceConfig>(
                        context.Configuration.GetSection(nameof(AppearanceConfig)));
                    services.Configure<ConnectionStringsCollectionConfig>(
                        context.Configuration.GetSection(nameof(ConnectionStringsCollectionConfig)));
                    services.Configure<DataStoragesCollectionConfig>(
                        context.Configuration.GetSection(nameof(DataStoragesCollectionConfig)));
                    services.Configure<PhiladelphusRepositoryHeadersCollectionConfig>(
                        context.Configuration.GetSection(nameof(PhiladelphusRepositoryHeadersCollectionConfig)));
                    services.Configure<MessagingConfig>(
                        context.Configuration.GetSection(nameof(MessagingConfig)));
                    services.Configure<IdentityConfig>(
                        context.Configuration.GetSection(nameof(IdentityConfig)));

                    // AutoMapper
                    var profileAssemblies = GetPhiladelphusProfileAssemblies();
                    services.AddAutoMapper(cfg =>
                    {
                        cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA2NDUxMjAwIiwiaWF0IjoiMTc3NDk5NzAxMyIsImFjY291bnRfaWQiOiIwMTlkNDYxMDhjYzI3YThhOGRlZmM3M2E1MWM4MzEwYSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa24zMTIwdzl0Zm1kNGFldDdoMWZka3lkIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.0nAJKYuxbe-byA9zSIMj65s1RDEQNkbqI3s3ypLkF11PRA8DZqR1gIgPAg4xE6LbqB5F0AEHXA6OBaUFxNyfS-prwCEB2LMRV9IbFPQEHkmZojcb_ygjldo1BtPQwBwVpjRsxnHOGQqJ1CGVCvF7F8TvnMjANRJVhEjsDh0OLyar3sz-Hun0GPRC6bIABmQh3fjOrD2WLJyIx2uW8dypdXnFpctOhTRMV8d3p8VfvOdkx70UxPoLMwQFPtdF_CaYkvSt_7pXMbVCxswrxD4BmXmmzza_6cUUCqa1aOHu5sHj5z0sHEnQxblAWF4ioZTB99XmncVZ-x4aTh-mhcIBHg";
                    },
                    profileAssemblies);

                    // Kafka
                    services.AddKafkaProducer<MessagingUser>(context.Configuration.GetSection($"{nameof(KafkaOptions<MessagingUser>)}:{nameof(MessagingUser)}"));
                    services.AddKafkaConsumer<MessagingUser>(context.Configuration.GetSection($"{nameof(KafkaOptions<MessagingUser>)}:{nameof(MessagingUser)}"));
                    services.AddKafkaProducer<Notification>(context.Configuration.GetSection($"{nameof(KafkaOptions<Notification>)}:{nameof(Notification)}"));
                    services.AddKafkaConsumer<Notification>(context.Configuration.GetSection($"{nameof(KafkaOptions<Notification>)}:{nameof(Notification)}"));

                    // Redis
                    services.AddStackExchangeRedisCache(options =>
                    {
                        var settings = context.Configuration
                            .GetSection(nameof(ApplicationSettingsConfig))
                            .Get<ApplicationSettingsConfig>()
                            ?? throw new InvalidOperationException("ApplicationSettingsConfig не инициализирован");

                        options.ConfigurationOptions = new ConfigurationOptions
                        {
                            EndPoints = { settings.RedisOptions.Socket },
                            Password = string.IsNullOrEmpty(settings.RedisOptions.Password)
                                ? null
                                : settings.RedisOptions.Password,
                            AbortOnConnectFail = false,
                            ConnectRetry = 1,
                            ConnectTimeout = 1000,
                            SyncTimeout = 1000,
                            AsyncTimeout = 1000
                        };
                        options.InstanceName = "Philadelphus:";
                    });
                    services.AddSingleton<IPhiladelphusRepositoryContentCache, DistributedPhiladelphusRepositoryContentCache>();

                    // Слой Core
                    services.AddSingleton<IUserService>(sp =>
                    {
                        var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityConfig>>();
                        return new UserService(options.Value.UserName);
                    });
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<IDataStoragesService, DataStoragesService>();
                    services.AddTransient<IPhiladelphusRepositoryCollectionService, PhiladelphusRepositoryCollectionService>();
                    services.AddTransient<IPhiladelphusRepositoryService, PhiladelphusRepositoryService>();
                    services.AddTransient<IExtensionManager, ExtensionManager>();
                    services.AddSingleton<IReportService, ReportService>();
                    services.AddSingleton<ITablesExportServiceFactory, TablesExportServiceFactory>();
                    RegisterFormulaEngine(services);

                    // Слой Presentation (платформенные реализации Avalonia)
                    services.AddSingleton<IDialogService, AvaloniaDialogService>();
                    services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
                    services.AddSingleton<AvaloniaWindowService>();
                    services.AddSingleton<IWindowService>(sp => sp.GetRequiredService<AvaloniaWindowService>());

                    // Фабрики команд: Avalonia-реализация с переопросом доступности через
                    // AvaloniaCommandManager (аналог WPF CommandManager.RequerySuggested). Общий
                    // DefaultRelayCommandFactory не годится — у него нет авто-переопроса CanExecute.
                    services.AddSingleton<IRelayCommandFactory, AvaloniaRelayCommandFactory>();
                    services.AddSingleton<IAsyncRelayCommandFactory, AvaloniaAsyncRelayCommandFactory>();

                    // Тема оформления: платформенная реализация (Avalonia) + VM выбора для ленты «Вид».
                    services.AddSingleton<IThemeService, AvaloniaThemeService>();
                    services.AddTransient<ThemeSettingsVM>();

                    // Презентация: ViewModel, фабрики VM, Excel-импорт (shared)
                    services.AddPhiladelphusPresentation();

                    // Окна
                    services.AddTransient<MainWindow>();
                    services.AddSingleton<LaunchWindow>();      // Singleton — для корректного скрытия/повторного показа
                    services.AddTransient<FormulaEditorWindow>();
                    services.AddTransient<AttributeValuesCollectionWindow>();
                    services.AddTransient<DetailsWindow>();
                    services.AddTransient<AboutWindow>();

                    // Фабрика инфраструктуры (зависит от конкретной Persistence — точка композиции)
                    services.AddTransient<IInfrastructureRepositoryFactory, InfrastructureRepositoryFactory>();

                    // Импорт из Excel
                    RegisterImportExport(services);
                })
                .Build();
        }

        private static void RegisterImportExport(IServiceCollection services)
        {
            RegisterImportExportCore(services);
            RegisterImportExportAdapters(services);
            RegisterExcelImportInfrastructure(services);
            RegisterExcelImportPresentation(services);
        }

        private static void RegisterFormulaEngine(IServiceCollection services)
        {
            services.AddSingleton<IFormulaProvider, ArithmeticFormulaProvider>();
            services.AddSingleton<IFormulaProvider, ComparisonFormulaProvider>();
            services.AddSingleton<IFormulaProvider, TextFormulaProvider>();
            services.AddSingleton<IFormulaProvider, ConditionalFormulaProvider>();
            services.AddSingleton<IFormulaProvider, TreeLeaveFormulaProvider>();

            services.AddSingleton(serviceProvider =>
            {
                var registry = new FormulaRegistry();
                foreach (var provider in serviceProvider.GetServices<IFormulaProvider>())
                {
                    registry.RegisterProvider(provider);
                }

                return registry;
            });

            services.AddSingleton<FormulaAstEvaluator>();
            services.AddSingleton<IFormulaDiagnosticsReporter, FormulaDiagnosticsReporter>();
        }

        private static void RegisterImportExportCore(IServiceCollection services)
        {
            services.AddTransient<IImportExportService, ImportExportService>();
        }

        private static void RegisterImportExportAdapters(IServiceCollection services)
        {
            services.AddTransient<IImportExportAdapter, JsonImportExportAdapter>();
        }

        private static void RegisterExcelImportInfrastructure(IServiceCollection services)
        {
            services.AddSingleton<ConversionService>();
            services.AddSingleton<ExcelPreviewService>();
            services.AddSingleton<IExcelDataTypeDetector, ExcelDataTypeDetector>();
            services.AddSingleton<IExcelImportSourceReader, ExcelImportSourceReader>();
            services.AddSingleton<IExcelImportSchemaBuilder, ExcelImportSchemaBuilder>();
            services.AddSingleton<IExcelImportProfileResolver, ExcelImportProfileResolver>();
            services.AddSingleton<IExcelImportProfileValidator, ExcelImportProfileValidator>();
            services.AddSingleton<IExcelImportInheritanceResolver, ExcelImportInheritanceResolver>();
            services.AddSingleton<IExcelImportSettingsReader, ExcelImportSettingsReader>();
            services.AddSingleton<IExcelImportSchemaTemplateStorage, ExcelImportSchemaTemplateStorage>();

            services.AddTransient<IImportExportAdapter, ExcelImportExportAdapter>();
            services.AddTransient<ExcelImportExportAdapter>();
            services.AddTransient<ExcelImportPipeline>();
            services.AddTransient<ExcelImportSessionState>();
        }

        private static void RegisterExcelImportPresentation(IServiceCollection services)
        {
            services.AddSingleton<IFileDialogService, AvaloniaFileDialogService>();
            services.AddSingleton<IMessageDialogService, AvaloniaMessageDialogService>();
            services.AddTransient<IImportProgressReporter, AvaloniaImportProgressReporter>();

            services.AddTransient<ImportFromExcelWindow>();
            services.AddTransient<ExcelImportDesignerWindow>();
            services.AddTransient<ImportProgressWindow>();
        }

        private static Assembly[] GetPhiladelphusProfileAssemblies()
        {
            var assembliesByName = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => IsPhiladelphusAssemblyName(assembly.GetName()))
                .GroupBy(x => x.GetName().Name)
                .ToDictionary(x => x.Key!, x => x.First());

            var referencesToLoad = new Queue<AssemblyName>(
                Assembly.GetExecutingAssembly()
                    .GetReferencedAssemblies()
                    .Where(IsPhiladelphusAssemblyName));

            while (referencesToLoad.Count > 0)
            {
                var assemblyName = referencesToLoad.Dequeue();
                if (assemblyName.Name == null || assembliesByName.ContainsKey(assemblyName.Name))
                {
                    continue;
                }

                var assembly = Assembly.Load(assemblyName);
                assembliesByName[assemblyName.Name] = assembly;

                foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies().Where(IsPhiladelphusAssemblyName))
                {
                    if (referencedAssemblyName.Name != null && assembliesByName.ContainsKey(referencedAssemblyName.Name) == false)
                    {
                        referencesToLoad.Enqueue(referencedAssemblyName);
                    }
                }
            }

            return assembliesByName.Values
                .Where(ContainsAutoMapperProfile)
                .ToArray();
        }

        private static bool IsPhiladelphusAssemblyName(AssemblyName assemblyName)
            => assemblyName.Name?.StartsWith("Philadelphus.", StringComparison.Ordinal) == true;

        private static bool ContainsAutoMapperProfile(Assembly assembly)
        {
            IEnumerable<Type> loadableTypes;
            try
            {
                loadableTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                loadableTypes = ex.Types.Where(type => type != null)!;
            }

            return loadableTypes
                .Any(type => typeof(Profile).IsAssignableFrom(type)
                    && type.Namespace?.StartsWith("Philadelphus.", StringComparison.Ordinal) == true);
        }
    }
}
