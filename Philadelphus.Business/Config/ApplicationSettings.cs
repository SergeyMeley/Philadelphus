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
    }
}
