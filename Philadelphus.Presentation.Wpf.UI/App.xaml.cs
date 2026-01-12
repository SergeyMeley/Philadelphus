using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    

                    // Дополнительные конфигурационные файлы
                    var additionalConfigsPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\AppData\\Local\\Philadelphus\\Configuration");
                    if (Directory.Exists(additionalConfigsPath) == false)
                    {
                        try
                        {
                            Directory.CreateDirectory(additionalConfigsPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка поиска или создания директории основных настроечных файлов {additionalConfigsPath}, обратитесь к разработчику. \r\n Подробности:\r\n{ex.Message}\r\n{ex.StackTrace}");
                            throw;
                        }
                    }
                    if (Directory.Exists(additionalConfigsPath))  // TODO: ВРЕМЕННО!!
                    {
                        var storageConfigPath = Path.Combine(additionalConfigsPath, "storages-config.json");
                        if (File.Exists(storageConfigPath) == false)
                        {
                            try
                            {
                                File.Create(storageConfigPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка поиска или создания настроечного файла хранилищ данных {storageConfigPath}, обратитесь к разработчику. \r\n Подробности:\r\n{ex.Message}\r\n{ex.StackTrace}");
                                throw;
                            }
                        }

                        var repositoryHeadersConfigPath = Path.Combine(additionalConfigsPath, "repository-headers-config.json");
                        if (File.Exists(repositoryHeadersConfigPath) == false)
                        {
                            try
                            {
                                File.Create(repositoryHeadersConfigPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка поиска или создания настроечного файла заголовков репозиториев {repositoryHeadersConfigPath}, обратитесь к разработчику. \r\n Подробности:\r\n{ex.Message}\r\n{ex.StackTrace}");
                                throw;
                            }
                        }

                        config.AddJsonFile(storageConfigPath, optional: true);
                        config.AddJsonFile(repositoryHeadersConfigPath, optional: true);
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
                await _host.StartAsync();

                var window = _host.Services.GetRequiredService<LaunchWindow>();
                window.Topmost = true;
                window.Show();
                window.Activate();
                window.Topmost = false;
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска приложения, обратитесь к разработчику.\r\nПодробнее:\r\n{ex.Message}\r\nТрассировка:\r\n{ex.StackTrace}");
                Shutdown(-1);
            }
            
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
