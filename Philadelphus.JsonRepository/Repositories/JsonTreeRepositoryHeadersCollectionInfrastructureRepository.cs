using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.JsonRepository.Repositories
{
    public class JsonTreeRepositoryHeadersCollectionInfrastructureRepository : ITreeRepositoryHeadersCollectionInfrastructureRepository
    {
        FileInfo _file = new FileInfo("repository-headers-config.json");
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.TreeRepositoryHeadersCollection; }
        public bool CheckAvailability()
        {
            if (_file.Exists == false)
                throw new FileNotFoundException($"Не найден перечень заголовков репозиториев: {_file.FullName}");
            return true;
        }

        public IEnumerable<TreeRepositoryHeader> SelectRepositoryCollection()
        {
            var filePath = "storage-config.json";

            if (CheckAvailability())
            {

            }
            TreeRepositoryHeadersCollection resultCollection = null;
            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };
            resultCollection = JsonSerializer.Deserialize<TreeRepositoryHeadersCollection>(json, options);
            return resultCollection.TreeRepositoryHeaders ?? throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");
        }

        public long UpdateRepositoryCollection(IEnumerable<TreeRepositoryHeader> collection)
        {
            throw new NotImplementedException();
        }
    }
}
