using System.Text.Json.Serialization;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Основные настройки приложения
    /// </summary>
    public class ApplicationSettings    //TODO: Подумать о переносе в Application
    {
        /// <summary>
        /// Директория конфигурационных файлов
        /// </summary>
        public string ConfigsDirectoryString { get; set; }

        /// <summary>
        /// Директория конфигурационных файлов 
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo ConfigsDirectory 
        { 
            get
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(ConfigsDirectoryString ?? string.Empty);
                return new DirectoryInfo(expandedPath);
            }
        }

        /// <summary>
        /// Полный путь конфигурационного файла хранилищ данных
        /// </summary>
        [JsonIgnore]
        public FileInfo StoragesConfigFullPath
        {
            get
            {
                var path = Path.Combine(ConfigsDirectoryString, "storages-config.json");
                var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
                return new FileInfo(expandedPath);
            }
        }

        /// <summary>
        /// Полный путь конфигурационного файла заголовков репозиториев
        /// </summary>
        [JsonIgnore]
        public FileInfo RepositoryHeadersConfigFullPath
        { get
            {
                var path = Path.Combine(ConfigsDirectoryString, "repository-headers-config.json");
                var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
                return new FileInfo(expandedPath);
            }
        }

        /// <summary>
        /// Коллекция директорий расположения расширений
        /// </summary>
        public List<string> PluginsDirectoriesString { get; set; }

        /// <summary>
        /// Массив директорий расположения расширений
        /// </summary>
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
