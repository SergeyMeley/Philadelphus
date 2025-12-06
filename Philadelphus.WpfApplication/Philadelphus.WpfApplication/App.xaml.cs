using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Business.Config;
using Philadelphus.Business.Mapping;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.WpfApplication.Models.StorageConfig;
using Philadelphus.WpfApplication.ViewModels;
using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Text.Json;
using System.Windows;

namespace Philadelphus.WpfApplication
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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Регистрация конфигурации
                    //var builder = new ConfigurationBuilder()
                    //    .SetBasePath(Directory.GetCurrentDirectory())
                    //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    //IConfiguration configuration = builder.Build();
                    services.Configure<ApplicationSettings>(context.Configuration.GetSection(nameof(ApplicationSettings)));

                    // Регистрация AutoMapper
                    services.AddAutoMapper(typeof(MappingProfile));

                    // Регистрация сервисов
                    //services.AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();     Заменено на IOptions<ApplicationSettings>
                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddSingleton<StorageConfigService>();      //TODO: Заменить на новый сервис
                    services.AddScoped<ITreeRepositoryCollectionService, TreeRepositoryCollectionService>();
                    services.AddScoped<ITreeRepositoryService, TreeRepositoryService>();
                    services.AddScoped<IExtensionManager, ExtensionManager>();

                    // Регистрация ViewModel
                    services.AddSingleton<ApplicationVM>();
                    services.AddSingleton<ApplicationCommandsVM>();
                    services.AddTransient<ApplicationWindowsVM>();
                    services.AddSingleton<LaunchWindowVM>();
                    services.AddTransient<MainWindowVM>();
                    services.AddSingleton<DataStoragesSettingsVM>();
                    services.AddScoped<RepositoryExplorerControlVM>();
                    services.AddScoped<TreeRepositoryCollectionVM>();
                    services.AddScoped<TreeRepositoryHeadersCollectionVM>();
                    services.AddScoped<RepositoryCreationControlVM>();
                    services.AddScoped<ExtensionControlVM>();

                    // Регистрация View
                    services.AddTransient<MainWindow>();
                    services.AddSingleton<LaunchWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var viewModel = _host.Services.GetRequiredService<ApplicationVM>();
            var window = _host.Services.GetRequiredService<LaunchWindow>();
            window.Show();
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
