using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Config;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using Philadelphus.Presentation.Wpf.UI.Factories.Implementations;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Models.StorageConfig;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Text.Json;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private ApplicationVM _viewModel;
        private readonly IHost _host;
        public App()
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            _host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                // Добавление конфигурационных файлов
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Основной конфигурационный файл
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    
                    var appDataPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");
                    if (Directory.Exists(appDataPath) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(appDataPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Не найдена директория основных настроечных файлов");
                            throw;
                        }
                        
                    }
                    if (Directory.Exists(appDataPath))
                    {
                        //config.AddJsonFile(Path.Combine(appDataPath, "storages-config.json"), optional: true);
                        //config.AddJsonFile(Path.Combine(appDataPath, "repository-headers-config.json"), optional: true);
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
                    //services.Configure<DataStoragesCollection>(
                    //    context.Configuration.GetSection(nameof(DataStoragesCollection)));
                    //services.Configure<DataStoragesCollection>(
                    //    context.Configuration.GetSection(nameof(TreeRepositoryHeadersCollection)));

                    // Регистрация AutoMapper
                    services.AddAutoMapper(typeof(MappingProfile));

                    // Регистрация сервисов
                    //services.AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();     Заменено на IOptions<ApplicationSettings>
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddScoped<IDataStoragesService, DataStoragesService>();
                    services.AddScoped<ITreeRepositoryCollectionService, TreeRepositoryCollectionService>();
                    services.AddScoped<ITreeRepositoryService, TreeRepositoryService>();
                    services.AddScoped<IExtensionManager, ExtensionManager>();

                    // Регистрация ViewModel
                    services.AddSingleton<ApplicationVM>();
                    services.AddSingleton<ApplicationCommandsVM>();
                    services.AddTransient<ApplicationWindowsVM>();
                    services.AddSingleton<LaunchWindowVM>();
                    //services.AddTransient<MainWindowVM>();    // Заменено на фабрику
                    services.AddSingleton<DataStoragesSettingsVM>();
                    //services.AddScoped<RepositoryExplorerControlVM>();    // Заменено на фабрику
                    services.AddScoped<TreeRepositoryCollectionVM>();
                    services.AddScoped<TreeRepositoryHeadersCollectionVM>();
                    services.AddScoped<RepositoryCreationControlVM>();

                    // Регистрация View
                    services.AddTransient<MainWindow>();
                    services.AddSingleton<LaunchWindow>();

                    // Регистрация фабрик
                    services.AddScoped<IMainWindowVMFactory, MainWindowVMFactory>();
                    services.AddScoped<IRepositoryExplorerControlVMFactory, RepositoryExplorerControlVMFactory>();
                    services.AddScoped<IExtensionsControlVMFactory, ExtensionsControlVMFactory>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var window = _host.Services.GetRequiredService<LaunchWindow>();
            window.Topmost = true;
            window.Show();
            window.Activate();
            window.Topmost = false;
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
