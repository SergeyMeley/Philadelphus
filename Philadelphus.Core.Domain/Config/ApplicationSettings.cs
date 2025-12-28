using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Config
{
    public class ApplicationSettings    //TODO: Подумать о переносе в Application
    {
        public string ConfigsDirectoryString { get; set; }

        [JsonIgnore]
        public DirectoryInfo ConfigsDirectory 
        { 
            get
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(ConfigsDirectoryString ?? string.Empty);
                return new DirectoryInfo(expandedPath);
            }
        }
        public List<string> PluginsDirectoriesString { get; set; }

        [JsonIgnore]
        public DirectoryInfo[] PluginsDirectories
        {
            get
            {
                var result = new DirectoryInfo[PluginsDirectoriesString.Count()];

                for (int i = 0; i < PluginsDirectoriesString.Count(); i++)
                {
                    var expandedPath = Environment.ExpandEnvironmentVariables(PluginsDirectoriesString[i] ?? string.Empty);
                    result[i] = new DirectoryInfo(expandedPath); 
                    
                }

                return result;
            }
        }
    }
}
