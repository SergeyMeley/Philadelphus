using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Philadelphus.Infrastructure.Persistence.Json.Repositories
{
    public class JsonDataStoragesCollectionInfrastructureRepository : IDataStoragesCollectionInfrastructureRepository
    {
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.DataStoragesCollection; }

        private FileInfo _file;
        public JsonDataStoragesCollectionInfrastructureRepository(
            FileInfo storagesConfigFullPath)
        {
            if (storagesConfigFullPath == null || storagesConfigFullPath.Exists == false)
                throw new Exception($"Некорректный путь к настроечному файлу: '{storagesConfigFullPath}'");
            _file = storagesConfigFullPath;
        }
        public bool CheckAvailability()
        {
            if (_file.Exists == false)
                throw new FileNotFoundException($"Не найден настроечный файл хранилищ данных: {_file.FullName}");
            return true;
        }

        public IEnumerable<DataStorage> SelectDataStorages()
        {
            DataStoragesCollectionConfig resultCollection = null;
            if (_file.Exists == false)
            {
                throw new FileNotFoundException($"Конфигурационный файл не найден: {_file.FullName}");
            }
            var json = File.ReadAllText(_file.FullName);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            resultCollection = JsonSerializer.Deserialize<DataStoragesCollectionConfig>(json, options);
            return resultCollection.DataStorages ?? throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");
        }

        public long UpdateDataStorages(IEnumerable<DataStorage> storages)
        {
            if (_file.Exists == false)
            {
                throw new FileNotFoundException($"Конфигурационный файл не найден: {_file.FullName}");
            }
            var collection = new DataStoragesCollectionConfig() { DataStorages = storages.ToList() };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize<DataStoragesCollectionConfig>(collection, options);
            File.WriteAllText(_file.FullName, json);

            return storages.Count();
        }
    }
}
