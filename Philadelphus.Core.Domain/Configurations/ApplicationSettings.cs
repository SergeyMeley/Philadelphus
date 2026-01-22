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
        /// Полный путь конфигурационного файла хранилищ данных
        /// </summary>
        public string StoragesConfigFullPathString { get; set; }

        /// <summary>
        /// Полный путь конфигурационного файла хранилищ данных
        /// </summary>
        [JsonIgnore]
        public FileInfo StoragesConfigFullPath
        {
            get
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(StoragesConfigFullPathString ?? string.Empty);
                return new FileInfo(expandedPath);
                //var path = Path.Combine(ConfigsDirectoryString, "storages-config-old.json");
                //var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
                //return new FileInfo(expandedPath);
            }
        }

        /// <summary>
        /// Полный путь конфигурационного файла заголовков репозиториев
        /// </summary>
        public string RepositoryHeadersConfigFullPathString { get; set; }

        /// <summary>
        /// Полный путь конфигурационного файла заголовков репозиториев
        /// </summary>
        [JsonIgnore]
        public FileInfo RepositoryHeadersConfigFullPath
        { 
            get
            {
                var expandedPath = Environment.ExpandEnvironmentVariables(RepositoryHeadersConfigFullPathString ?? string.Empty);
                return new FileInfo(expandedPath);
                //var path = Path.Combine(ConfigsDirectoryString, "repository-headers-config-old.json");
                //var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
                //return new FileInfo(expandedPath);
            }
        }

        /// <summary>
        /// Коллекция директорий расположения расширений
        /// </summary>
        public List<string> PluginsDirectoriesStrings { get; set; }

        /// <summary>
        /// Массив директорий расположения расширений
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo[] PluginsDirectories
        {
            get
            {
                var result = new DirectoryInfo[PluginsDirectoriesStrings.Count()];

                for (int i = 0; i < PluginsDirectoriesStrings.Count(); i++)
                {
                    var expandedPath = Environment.ExpandEnvironmentVariables(PluginsDirectoriesStrings[i] ?? string.Empty);
                    result[i] = new DirectoryInfo(expandedPath); 
                    
                }

                return result;
            }
        }
    }
}
