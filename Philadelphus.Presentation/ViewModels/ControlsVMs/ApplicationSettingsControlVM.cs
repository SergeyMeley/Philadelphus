using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.SettingsContainersVMs;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для настроек приложения.
    /// </summary>
    public class ApplicationSettingsControlVM : ControlBaseVM
    {
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IRelayCommandFactory _commandFactory;
        private readonly IAsyncRelayCommandFactory _asyncCommandFactory;
        private readonly IFileDialogService _fileDialogService;

        /// <summary>
        /// Настройки приложения.
        /// </summary>
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
       
        /// <summary>
        /// Выполняет операцию ConfigFiles.
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public HashSet<ConfigurationFileVM> ConfigFiles { get; } = new HashSet<ConfigurationFileVM>();
       
        /// <summary>
        /// Выбранный конфигурационный файл.
        /// </summary>
        public ConfigurationFileVM SelectedConfigFile { get; set; }
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationSettingsControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="configurationService">Параметр configurationService.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="appConfig">Параметр appConfig.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <param name="fileDialogService">Сервис файловых диалогов.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ApplicationSettingsControlVM(IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IConfigurationService configurationService,
            IApplicationCommandsVM applicationCommandsVM,
            IOptions<ApplicationSettingsConfig> appConfig,
            IRelayCommandFactory commandFactory,
            IAsyncRelayCommandFactory asyncCommandFactory,
            IFileDialogService fileDialogService)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(appConfig.Value);
            ArgumentNullException.ThrowIfNull(appConfig.Value.ConfigurationFilesPathes);
            ArgumentNullException.ThrowIfNull(commandFactory);
            ArgumentNullException.ThrowIfNull(asyncCommandFactory);
            ArgumentNullException.ThrowIfNull(fileDialogService);

            _configurationService = configurationService;
            _appConfig = appConfig;
            _commandFactory = commandFactory;
            _asyncCommandFactory = asyncCommandFactory;
            _fileDialogService = fileDialogService;

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

            InitializeTabs();
        }
        public IRelayCommand OpenConfigCommand
        {
            get
            {
                return _commandFactory.Create(obj =>
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
                        _notificationService.SendModalWindow<ApplicationSettingsControlVM>(
                            $"Не удалось открыть файл: {ex.Message}");
                    }
                });
            }
        }
        public IAsyncRelayCommand MoveConfigCommand
        {
            get
            {
                return _asyncCommandFactory.Create(async obj =>
                {
                    var path = string.Empty;
                    var selectedFolder = await _fileDialogService.BrowseFolderAsync(
                        "Выберите директорию",
                        SelectedConfigFile.FileInfo.Directory.FullName);

                    if (selectedFolder != null)
                    {
                        path = selectedFolder;
                    }
                    try
                    {
                        var originPath = SelectedConfigFile.FileInfo.DirectoryName;
                        _configurationService.MoveConfigFile(SelectedConfigFile.FileInfo, new DirectoryInfo(path));
                        _notificationService.SendModalWindow<ApplicationSettingsControlVM>(
                            $"Настроечный файл '{SelectedConfigFile.ConfigName}' перемещен\r\nиз\r\n'{originPath}'\r\nв\r\n'{SelectedConfigFile.FileInfo.DirectoryName}'",
                            NotificationCriticalLevelModel.Info);
                    }
                    catch (Exception)
                    {
                        _notificationService.SendModalWindow<ApplicationSettingsControlVM>(
                            "Ошибка перемещения файла, действие не выполнено. Обратитесь к разработчику.");
                    }

                    SelectedConfigFile.OnPropertyChanged(nameof(SelectedConfigFile.FilePath));
                });
            }
        }

        public IAsyncRelayCommand SelectAnotherConfigCommand
        {
            get
            {
                return _asyncCommandFactory.Create(async obj =>
                {
                    var path = string.Empty;
                    var selectedFile = await _fileDialogService.OpenFileAsync(
                        "JSON файлы (*.json)|*.json|" +
                            "Все файлы (*.*)|*.*",
                        null,
                        "Выберите директорию",
                        SelectedConfigFile.FileInfo.Directory.FullName);

                    if (selectedFile != null)
                    {
                        path = selectedFile;
                    }
                    try
                    {
                        var originPath = SelectedConfigFile.FileInfo.FullName;

                        _configurationService.SelectAnotherConfigFile(SelectedConfigFile, new FileInfo(path));
                        _notificationService.SendModalWindow<ApplicationSettingsControlVM>(
                            $"Настроечный файл '{SelectedConfigFile.ConfigName}' заменен\r\nс\r\n'{originPath}'\r\nна\r\n'{SelectedConfigFile.FilePath}'",
                            NotificationCriticalLevelModel.Info);
                    }
                    catch (Exception)
                    {
                        _notificationService.SendModalWindow<ApplicationSettingsControlVM>(
                            "Ошибка перемещения файла, действие не выполнено. Обратитесь к разработчику.");
                    }

                    SelectedConfigFile.OnPropertyChanged(nameof(SelectedConfigFile.FilePath));

                });
            }
        }
        private void InitializeTabs()
        {
            var tab1 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            tab1.Header = "Настроечные файлы";
            tab1.Content = this;

            //var tab3 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab3.Header = "Конфиденциальная информация";
            //tab3.Content = new () { DataContext = this };

            //var tab4 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab4.Header = "Учетная запись";
            //tab4.Content = new () { DataContext = this };

            //var tab5 = _serviceProvider.GetRequiredService<ApplicationSettingsTabItemControlVM>();
            //tab5.Header = "Сочетания клавиш";
            //tab5.Content = new HotKeysSettingsTabControl() { DataContext = this };

            ApplicationSettingsTabItemsVMs = new List<ApplicationSettingsTabItemControlVM> { tab1 };

            SelectedApplicationSettingsTabItemVM = ApplicationSettingsTabItemsVMs.FirstOrDefault();
        }
    }
}
