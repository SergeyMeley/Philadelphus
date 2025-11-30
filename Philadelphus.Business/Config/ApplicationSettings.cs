using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Config
{
    public class ApplicationSettings    //TODO: Подумать о переносе в Application
    {
        public string ConfigsDirectoryString { get; set; }
        public DirectoryInfo ConfigsDirectory 
        { 
            get
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(ConfigsDirectoryString ?? string.Empty);
                return new DirectoryInfo(expandedPath);
            }
        }
        public string[] PluginsDirectoriesString { get; set; }
        public DirectoryInfo[] PluginsDirectories
        {
            get
            {
                var result = new DirectoryInfo[PluginsDirectoriesString.Length];

                for (int i = 0; i < PluginsDirectoriesString.Length; i++)
                {
                    var expandedPath = Environment.ExpandEnvironmentVariables(PluginsDirectoriesString[i] ?? string.Empty);
                    result[i] = new DirectoryInfo(expandedPath); 
                    
                }

                return result;
            }
        }
    }
}
