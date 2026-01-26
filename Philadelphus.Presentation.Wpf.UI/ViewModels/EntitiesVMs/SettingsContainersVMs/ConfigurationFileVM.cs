using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.SettingsContainersVMs
{
    public class ConfigurationFileVM : ViewModelBase
    {
        public FileInfo FileInfo { get; private set; }
        public string ConfigName { get; init; }
        public string FilePath => FileInfo.FullName;
        public bool Exists { get => FileInfo.Exists; }
        public ConfigurationFileVM(string name, FileInfo fileInfo)
        {
            ConfigName = name;
            FileInfo = fileInfo;
        }
        public override bool Equals(object? obj)
        {
            if (obj is ConfigurationFileVM cf && cf?.ConfigName == this.ConfigName)
                return true;
            return false;
        }
    }
}
