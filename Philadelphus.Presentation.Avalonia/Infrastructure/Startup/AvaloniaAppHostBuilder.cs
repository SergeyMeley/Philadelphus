using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Identity.Configurations;
using Philadelphus.Core.Domain.Identity.Services.Implementations;
using Philadelphus.Core.Domain.Identity.Services.Interfaces;
using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.Relations;
using Philadelphus.Core.Domain.TablesExport.Factories;
using Philadelphus.Infrastructure.Cache.Redis.Implementations;
using Philadelphus.Infrastructure.Cache.RepositoryInterfaces;
using Philadelphus.Infrastructure.Messaging.Kafka;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Avalonia.Factories;
using Philadelphus.Presentation.Avalonia.Infrastructure;
using Philadelphus.Presentation.Avalonia.Services;
using Philadelphus.Presentation.Avalonia.Views.Windows;
using Philadelphus.Presentation.Configurations;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services;
using Philadelphus.Presentation.Services.Implementations;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;

using Serilog;

using StackExchange.Redis;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Startup
{
    internal static class AvaloniaAppHostBuilder
    {
        public static IHost Build()
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

            var configuration = builder.Build();

            return Host.CreateDefaultBuilder()
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(configuration);

                    var basePath = ConfigPathHelper.Expand(
                        configuration["ApplicationSettings:BasePath"]
                        ?? "%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");

                    Log.Information($"Проверка базовой директории: {basePath}");

                    ConfigurationService.CheckOrInitDirectory(new DirectoryInfo(basePath)).GetAwaiter().GetResult();

                    var configFiles = new Dictionary<string, object>
                    {
                        [ConfigPathHelper.Expand(
                            configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(ConnectionStringsCollectionConfig)}"]!)]
                            = new ConnectionStringsCollectionConfig { ConnectionStringsContainers = new() },
                        [ConfigPathHelper.Expand(
                            configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(DataStoragesCollectionConfig)}"]!)]
                            = new DataStoragesCollectionConfig { DataStorages = new() },
                        [ConfigPathHelper.Expand(
                            configuration[$"{nameof(ApplicationSettingsConfig)}:ConfigurationFilesPathesStrings:{nameof(PhiladelphusRepositoryHeadersCollectionConfig)}"]!)]
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
                    var profileAssemblies = AutoMapperProfileAssemblyProvider.GetPhiladelphusProfileAssemblies();
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
                    services.AddTransient<IRepositoryRelationsService, RepositoryRelationsService>();
                    services.AddTransient<IExtensionManager, ExtensionManager>();
                    services.AddSingleton<IReportService, ReportService>();
                    services.AddSingleton<ITablesExportServiceFactory, TablesExportServiceFactory>();
                    FormulaEngineServiceCollectionExtensions.RegisterFormulaEngine(services);

                    // Слой Presentation (платформенные реализации Avalonia)
                    services.AddSingleton<IDialogService, AvaloniaDialogService>();
                    services.AddSingleton<IDataStorageSelectionDialogService, AvaloniaDataStorageSelectionDialogService>();
                    services.AddSingleton<IRelationDeletionConfirmationService, AvaloniaRelationDeletionConfirmationService>();
                    services.AddSingleton<ILeavePolymorphismConfirmationService, AvaloniaLeavePolymorphismConfirmationService>();
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
                    ImportExportServiceCollectionExtensions.RegisterImportExport(services);
                })
                .Build();
        }
    }
}
