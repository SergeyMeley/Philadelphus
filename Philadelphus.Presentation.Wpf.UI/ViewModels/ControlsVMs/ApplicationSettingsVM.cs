using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ApplicationSettingsVM : ControlBaseVM
    {
        public HashSet<ConfogurationFileVM> ConfigFilesPathes { get; set; } = new HashSet<ConfogurationFileVM>();
        public ApplicationSettingsVM(IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            ApplicationCommandsVM applicationCommandsVM,
            IOptions<ApplicationSettings> appConfig)
            : base(serviceProvider, logger, notificationService, applicationCommandsVM)
        {
            ConfigFilesPathes.Add(new ConfogurationFileVM("Настроечный файл хранилищ данных", appConfig.Value.StoragesConfigFullPathString));
            ConfigFilesPathes.Add(new ConfogurationFileVM("Настроечный файл заголовков репозиториев", appConfig.Value.RepositoryHeadersConfigFullPathString));
        }
    }

    public class ConfogurationFileVM
    {
        public string Name { get; init; }
        public FileInfo FileInfo { get; init; }
        public bool Exists { get => FileInfo.Exists; }
        public ConfogurationFileVM(string name, string fileFullPath)
        {
            Name = name;
            FileInfo = new FileInfo(fileFullPath);
        }
        public ConfogurationFileVM(string name, FileInfo fileInfo)
        {
            Name = name;
            FileInfo = fileInfo;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ConfogurationFileVM cf && cf.Name == this.Name)
                return true;
            return false;
        }
    }
}
