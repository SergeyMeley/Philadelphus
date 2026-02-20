using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.TabItemsControls.ApplicationSettingsTabItemsControls;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.TabItemsControls.LaunchWindowTabItemsControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public List<ApplicationSettingsTabItemControlVM> ApplicationSettingsTabItemsVMs { get; set; }

        private ApplicationSettingsTabItemControlVM _selectedApplicationSettingsTabItemVM;
        public ApplicationSettingsTabItemControlVM SelectedApplicationSettingsTabItemVM
        {
            get => _selectedApplicationSettingsTabItemVM;
            set
            {
                if (_selectedApplicationSettingsTabItemVM != value)
                {
                    _selectedApplicationSettingsTabItemVM = value;
                    OnPropertyChanged();
                }
            }
        }
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

            Dictionary<string, Type> existingTypes = new()
                {
                    { nameof(ConnectionStringsCollectionConfig), typeof(ConnectionStringsCollectionConfig) },
                    { nameof(DataStoragesCollectionConfig), typeof(DataStoragesCollectionConfig) },
                    { nameof(PhiladelphusRepositoryHeadersCollectionConfig), typeof(PhiladelphusRepositoryHeadersCollectionConfig) }
                };

            foreach (var configFile in _appConfig.Value.ConfigurationFilesPathes)
            {
                string displayName = string.Empty;

                if (existingTypes.TryGetValue(configFile.Key, out var type) == false)
                {
                    type = Type.GetType(configFile.Key);
                    if (type == null)
                    {
                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("Philadelphus")))
                        {
                            type = Array.Find(assembly.GetTypes(), t => t.Name.Equals(configFile.Key) || t.Name == configFile.Key);
                            if (type != null)
                                break;
                        }
                    }
                }

                displayName = type?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                        ?? type?.Name
                        ?? configFile.Key;

                ConfigFiles.Add(new ConfigurationFileVM(displayName, configFile.Value));
            }

            foreach (var cs in _connectionStringsCollectionConfig.Value.ConnectionStringContainers) 
            {
                ConnectionStringContainersVMs.Add(_mapper.Map<ConnectionStringContainerVM>(cs));
            }

            InitializeTabs();
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
                        MessageBox.Show($"Настроечный файл '{SelectedConfigFile.ConfigName}' перемещен\r\nиз\r\n'{originPath}'\r\nв\r\n'{SelectedConfigFile.FileInfo.DirectoryName}'");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Ошибка перемещения файла, действие не выполнено. Обратитесь к разработчику.");
                    }
                    
                    SelectedConfigFile.OnPropertyChanged(nameof(SelectedConfigFile.FilePath));
                });
            }
        }

        public RelayCommand SelectAnotherConfigCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var path = string.Empty;
                    var dialog = new OpenFileDialog
                    {
                        Title = "Выберите директорию",
                        Multiselect = false,
                        Filter = "JSON файлы (*.json)|*.json|" +
                                    "Все файлы (*.*)|*.*",
                        FilterIndex = 1,
                        InitialDirectory = SelectedConfigFile.FileInfo.Directory.FullName,
                        DefaultDirectory = SelectedConfigFile.FileInfo.Directory.FullName,
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        path = dialog.FileName;
                    }
                    try
                    {
                        var originPath = SelectedConfigFile.FileInfo.FullName;

                        _configurationService.SelectAnotherConfigFile(SelectedConfigFile, new FileInfo(path));
                        MessageBox.Show($"Настроечный файл '{SelectedConfigFile.ConfigName}' заменен\r\nс\r\n'{originPath}'\r\nна\r\n'{SelectedConfigFile.FilePath}'");
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
                return new RelayCommand(
                    obj =>
                    {
                        var vm = new ConnectionStringContainerVM()
                        {
                            Uuid = Guid.NewGuid(),
                            ProviderName = NewConnectionStringContainerSelectedProvider,
                            ConnectionString = NewConnectionStringContainerConnectionString
                        };
                        ConnectionStringContainersVMs.Add(vm);
                        SaveConnectionStringContainersCommand.Execute(obj);
                    },
                    ce =>
                    {
                        if (string.IsNullOrEmpty(NewConnectionStringContainerSelectedProvider))
                            return false;
                        if (string.IsNullOrEmpty(NewConnectionStringContainerConnectionString))
                            return false;
                        return true;
                    });
            }
        }
        public RelayCommand SaveConnectionStringContainersCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    // Изменение существующих строк подключения
                    for (int i = 0; i < _connectionStringsCollectionConfig.Value.ConnectionStringContainers.Count; i++)
                    {
                        var cs = _connectionStringsCollectionConfig.Value.ConnectionStringContainers[i];
                        var vm = ConnectionStringContainersVMs.SingleOrDefault(x => x.Uuid == cs.Uuid);
                        if (vm != null)
                        {
                            _connectionStringsCollectionConfig.Value.ConnectionStringContainers[i] = _mapper.Map<ConnectionStringContainer>(vm);
                        }
                    }

                    // Добавление новых строк подключения
                    var newVms = ConnectionStringContainersVMs.Where(x => _connectionStringsCollectionConfig.Value.ConnectionStringContainers.Any(c => c.Uuid == x.Uuid) == false);
                    foreach (var newVm in newVms)
                    {
                        _connectionStringsCollectionConfig.Value.ConnectionStringContainers.Add(_mapper.Map<ConnectionStringContainer>(newVm));
                    }

                    // Исключение удаленных строк подключения
                    foreach (var vm in ConnectionStringContainersVMs.Where(x => x.ForDelete == true))
                    {
                        var cs = _connectionStringsCollectionConfig.Value.ConnectionStringContainers.SingleOrDefault(x => x.Uuid == vm.Uuid);
                        _connectionStringsCollectionConfig.Value.ConnectionStringContainers.Remove(cs);
                    }
                    for (int i = ConnectionStringContainersVMs.Count - 1; i >= 0; i--)
    {
                        if (ConnectionStringContainersVMs[i].ForDelete == true)
        {
                            ConnectionStringContainersVMs.RemoveAt(i);
                        }
                    }

                    // Обновление настроечного файла
                    if (_appConfig.Value.TryGetConfigFileFullPath<ConnectionStringsCollectionConfig>(out var config))
                    {
                        _configurationService.UpdateConfigFile(config, _connectionStringsCollectionConfig);
                    }
                    else
                    {
                        var message = "Путь к конфигурационному файлу 'ConnectionStringsCollectionConfig' не найден, изменения не сохранены";
                        _logger.LogError(message);
                        MessageBox.Show(message);
                    }
                });
            }
        }

        public RelayCommand DeleteConnectionStringContainersCommand
        {
            get
            {
                return new RelayCommand(obj =>
        {
                    SelectedConnectionStringContainerVM.ForDelete = true;
                });
            }
        }
        private void InitializeTabs()
        {
            var tab1 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            tab1.Header = "Настроечные файлы";
            tab1.Content = new ConfigFilesPathesTabControl() { DataContext = this };

            var tab2 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            tab2.Header = "Строки подключения";
            tab2.Content = new ConnectionStringsTabControl() { DataContext = this };

            //var tab3 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab3.Header = "Конфиденциальная информация";
            //tab3.Content = new () { DataContext = this };

            //var tab4 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab4.Header = "Учетная запись";
            //tab4.Content = new () { DataContext = this };

            //var tab5 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab5.Header = "Сочетания клавиш";
            //tab5.Content = new HotKeysSettingsTabControl() { DataContext = this };

            ApplicationSettingsTabItemsVMs = new List<ApplicationSettingsTabItemControlVM> { tab1, tab2 };

            SelectedApplicationSettingsTabItemVM = ApplicationSettingsTabItemsVMs.FirstOrDefault(t => t.Content is ConfigFilesPathesTabControl);
        }
    }
}
