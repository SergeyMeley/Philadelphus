using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ApplicationSettingsControlVM : ControlBaseVM
    {
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;

        public HashSet<ConfigurationFileVM> ConfigFiles { get; set; } = new HashSet<ConfigurationFileVM>();
        public ConfigurationFileVM SelectedConfigFile { get; set; }
        public ApplicationSettingsControlVM(IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IConfigurationService configurationService,
            ApplicationCommandsVM applicationCommandsVM,
            IOptions<ApplicationSettingsConfig> appConfig)
            : base(serviceProvider, logger, notificationService, applicationCommandsVM)
        {
            _configurationService = configurationService;
            _appConfig = appConfig;

            ConfigFiles.Add(new ConfigurationFileVM("Настроечный файл хранилищ данных", appConfig.Value.StoragesConfigFullPath, appConfig));
            ConfigFiles.Add(new ConfigurationFileVM("Настроечный файл заголовков репозиториев", appConfig.Value.RepositoryHeadersConfigFullPath, appConfig));
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

    }

    public class ConfigurationFileVM : ViewModelBase
    {
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        public FileInfo FileInfo { get; private set; }
        public string ConfigName { get; init; }
        public string FilePath => FileInfo.Directory.FullName;
        public bool Exists { get => FileInfo.Exists; }
        public ConfigurationFileVM(string name, FileInfo fileInfo, IOptions<ApplicationSettingsConfig> appConfig)
        {
            ConfigName = name;
            FileInfo = fileInfo;
            _appConfig = appConfig;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ConfigurationFileVM cf && cf?.ConfigName == this.ConfigName)
                return true;
            return false;
        }
    }
}
