using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Configurations;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text.Json.Serialization;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Основные настройки приложения
    /// </summary>
    public class ApplicationSettingsConfig    //TODO: Подумать о переносе в Application
    {
        /// <summary>
        /// Пути к конфигурационным файлам
        /// </summary>
        public Dictionary<string, string> ConfigurationFilesPathesStrings { get; set; }

        /// <summary>
        /// Пути к конфигурационным файлам
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, FileInfo> ConfigurationFilesPathes 
        {
            get
            {
                var result = new Dictionary<string, FileInfo>();
                foreach(var path in ConfigurationFilesPathesStrings)
                {
                    result.Add(path.Key, GetFileInfo(path.Value));
                }
                return result;
            } 
        }

        /// <summary>
        /// Директория конфигурационных файлов
        /// </summary>
        public string ConfigsDirectoryString { get; set; }

        /// <summary>
        /// Полный путь конфигурационного файла хранилищ данных
        /// </summary>
        [JsonIgnore]
        public FileInfo ConnectionStringsConfigFullPath
        {
            get
            {
                TryGetConfigFileFullPath<ConnectionStringsCollectionConfig>(out var result);
                return result;
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
                TryGetConfigFileFullPath<DataStoragesCollectionConfig>(out var result);
                return result;
            }
        }

        /// <summary>
        /// Полный путь конфигурационного файла заголовков репозиториев
        /// </summary>
        [JsonIgnore]
        public FileInfo RepositoryHeadersConfigFullPath
        {
            get
            {
                TryGetConfigFileFullPath<TreeRepositoryHeadersCollectionConfig>(out var result);
                return result;
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
                    result[i] = GetDirectoryInfo(PluginsDirectoriesStrings[i]); 
                }

                return result;
            }
        }

        /// <summary>
        /// Настройки подключения к Redis
        /// </summary>
        public RedisOptions RedisOptions { get; set; }

        /// <summary>
        /// Настройки подключения к Redis
        /// </summary>
        public KafkaOptions KafkaOptions { get; set; }

        /// <summary>
        /// Попытаться получить конфигурационный файл
        /// </summary>
        /// <typeparam name="TConfig">Тип конфигурации для получения ключа поиска пути в основном настроечном файле</typeparam>
        /// <param name="fileInfo">Информация об искомом настроечном файле</param>
        /// <returns>Успешность нахождения</returns>
        public bool TryGetConfigFileFullPath<TConfig>(out FileInfo fileInfo)
        {
            return ConfigurationFilesPathes.TryGetValue(typeof(TConfig).Name, out fileInfo);
        }

        private FileInfo GetFileInfo(string path)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
            var result = new FileInfo(expandedPath);
            return result;
        }
        private DirectoryInfo GetDirectoryInfo(string path)
        {
            var expandedPath = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
            var result = new DirectoryInfo(expandedPath);
            return result;
        }
    }
}
