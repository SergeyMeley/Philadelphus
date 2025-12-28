using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Json.Repositories
{
    public class JsonDataStoragesCollectionInfrastructureRepository : IDataStoragesCollectionInfrastructureRepository
    {
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.DataStoragesCollection; }

        private FileInfo _file;
        public JsonDataStoragesCollectionInfrastructureRepository(DirectoryInfo directory)
        {
            _file = new FileInfo(Path.Combine(directory.FullName, "storage-config.json"));
        }
        public bool CheckAvailability()
        {
            if (_file.Exists == false)
                throw new FileNotFoundException($"Не найден настроечный файл хранилищ данных: {_file.FullName}");
            return true;
        }

        public IEnumerable<DataStorage> SelectDataStorages()
        {
            var filePath = "storage-config.json";
            DataStoragesCollection resultCollection = null;
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Конфигурационный файл не найден: {filePath}");
            }
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            resultCollection = JsonSerializer.Deserialize<DataStoragesCollection>(json, options);
            return resultCollection.DataStorages ?? throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");
        }

        public long UpdateDataStorages(IEnumerable<DataStorage> storages)
        {
            var filePath = "storages.json";
            var collection = new DataStoragesCollection() { DataStorages = storages.ToList() };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize<DataStoragesCollection>(collection, options);
            File.WriteAllText(filePath, json);

            return storages.Count();
        }
    }
}
