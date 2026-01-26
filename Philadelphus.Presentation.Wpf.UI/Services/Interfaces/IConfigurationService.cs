using Microsoft.Extensions.Options;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Services.Interfaces
{
    public interface IConfigurationService
    {
        public bool MoveConfigFile(FileInfo configFile, DirectoryInfo newDirectory);
        public bool SelectAnotherConfigFile(ConfigurationFileVM configurationFileVM, FileInfo newFile);
        public bool UpdateConfigFile<T>(FileInfo configFile, IOptions<T> newConfigObject) where T : class;
    }
}
