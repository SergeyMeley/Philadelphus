using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ApplicationSettingsControlVM : ControlBaseVM
    {
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollectionConfig;
        public HashSet<ConfigurationFileVM> ConfigFiles { get; } = new HashSet<ConfigurationFileVM>();
        public ConfigurationFileVM SelectedConfigFile { get; set; }
        public ObservableCollection<ConnectionStringContainerVM> ConnectionStringContainersVMs { get; } = new ObservableCollection<ConnectionStringContainerVM>();
        public ConnectionStringContainerVM SelectedConnectionStringContainerVM {  get; set; } 
        public string[] ProvidersNames
        { 
            get
            {
                return new[] {
                    "Npgsql.EntityFrameworkCore.PostgreSQL",
                    "System.Text.Json.JsonSerializer"
                };
            }
        }

        public string NewConnectionStringContainerSelectedProvider { get; set; }
        public string NewConnectionStringContainerConnectionString { get; set; }


        public ApplicationSettingsControlVM(IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IConfigurationService configurationService,
            ApplicationCommandsVM applicationCommandsVM,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollectionConfig)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _configurationService = configurationService;
            _appConfig = appConfig;
            _connectionStringsCollectionConfig = connectionStringsCollectionConfig;

            ConfigFiles.Add(new ConfigurationFileVM("Настроечный файл хранилищ данных", appConfig.Value.StoragesConfigFullPath));
            ConfigFiles.Add(new ConfigurationFileVM("Настроечный файл заголовков репозиториев", appConfig.Value.RepositoryHeadersConfigFullPath));

            foreach (var cs in _connectionStringsCollectionConfig.Value.ConnectionStringContainers) 
            {
                ConnectionStringContainersVMs.Add(_mapper.Map<ConnectionStringContainerVM>(cs));
            }
        }
        public RelayCommand OpenConfigCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    try
                    {
                        // Открыть в программе по умолчанию (Notepad, VS Code, etc.)
                        Process.Start(new ProcessStartInfo(SelectedConfigFile.FileInfo.FullName)
                        {
                            UseShellExecute = true  // Использует ассоциацию Windows
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}");
                    }
                });
            }
        }
        public RelayCommand MoveConfigCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var path = string.Empty;
                    var dialog = new OpenFolderDialog
                    {
                        Title = "Выберите директорию",
                        Multiselect = false,
                        InitialDirectory = SelectedConfigFile.FileInfo.Directory.FullName,
                        DefaultDirectory = SelectedConfigFile.FileInfo.Directory.FullName,
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        path = dialog.FolderName;
                    }
                    try
                    {
                        var originPath = SelectedConfigFile.FileInfo.DirectoryName;
                        _configurationService.MoveConfigFile(SelectedConfigFile.FileInfo, new DirectoryInfo(path));
                        MessageBox.Show($"Настроечный файл '{SelectedConfigFile.ConfigName}' перемещен\r\nиз\r\n'{originPath}'\r\nв\r\n'{SelectedConfigFile.FilePath}'");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Ошибка перемещения файла, действие не выполнено. Обратитесь к разработчику.");
                    }
                    
                    SelectedConfigFile.OnPropertyChanged(nameof(SelectedConfigFile.FilePath));
                });
            }
        }
        public RelayCommand CreateAndSaveNewConnectionStringContainerCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var cs = new ConnectionStringContainer()
                    {
                        Uuid = Guid.NewGuid(),
                        ProviderName = NewConnectionStringContainerSelectedProvider,
                        ConnectionString = NewConnectionStringContainerSelectedConnectionString
                    };
                    var vm = _mapper.Map<ConnectionStringContainerVM>(cs);
                    ConnectionStringContainersVMs.Add(vm);
                    return;
                },
                ce =>
                {
                    if (string.IsNullOrEmpty(NewConnectionStringContainerSelectedProvider))
                        return false;
                    if (string.IsNullOrEmpty(NewConnectionStringContainerSelectedConnectionString))
                        return false;
                    return true;
                });
            }
        }
    }
}
