using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Philadelphus.Business.Mapping;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.WpfApplication.ViewModels;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System.Configuration;
using System.Data;
using System.Globalization;
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
            .ConfigureServices((context, services) =>
            {
                // Регистрация AutoMapper
                services.AddAutoMapper(typeof(MappingProfile));

                // Регистрация сервисов
                services.AddSingleton<IApplicationSettingsService, ApplicationSettingsService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddScoped<ITreeRepositoryCollectionService, TreeRepositoryCollectionService>();
                services.AddScoped<ITreeRepositoryService, TreeRepositoryService>();

                // Регистрация ViewModel
                services.AddScoped<ApplicationVM>();
                services.AddScoped<ApplicationCommandsVM>();
                services.AddScoped<ApplicationWindowsVM>();
                services.AddScoped<LaunchVM>();
                services.AddScoped<DataStoragesSettingsVM>();
                services.AddScoped<TreeRepositoryCollectionVM>();
                services.AddScoped<TreeRepositoryHeadersCollectionVM>();
                services.AddScoped<RepositoryCreationVM>();

                // Регистрация View
                services.AddSingleton<MainWindow>();
                services.AddSingleton<LaunchWindow>();
            })
            .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            //_viewModel = _host.Services.GetRequiredService<ApplicationVM>();
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
