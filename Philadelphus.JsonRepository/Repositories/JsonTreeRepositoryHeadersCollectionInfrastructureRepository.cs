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
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.TreeRepositoryHeadersCollection; }

        private FileInfo _file;
        public JsonTreeRepositoryHeadersCollectionInfrastructureRepository(DirectoryInfo directory)
        {
            _file = new FileInfo(Path.Combine(directory.FullName, "repository-headers-config.json"));
        }
        public bool CheckAvailability()
        {
            if (_file.Exists == false)
                throw new FileNotFoundException($"Не найден перечень заголовков репозиториев: {_file.FullName}");
            return true;
        }

        public IEnumerable<TreeRepositoryHeader> SelectRepositoryCollection()
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeRepositoryHeader> result = null;

            var json = File.ReadAllText(_file.FullName);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };

            result = JsonSerializer.Deserialize<TreeRepositoryHeadersCollection>(json, options).TreeRepositoryHeaders;

            if (result == null)
                throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");

            return result;
        }

        public long UpdateRepository(TreeRepositoryHeader treeRepositoryHeader)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            var json = File.ReadAllText(_file.FullName);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };

            var treeRepositoryHeadersCollection = JsonSerializer.Deserialize<TreeRepositoryHeadersCollection>(json, options);

            if (treeRepositoryHeadersCollection == null)
                throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");


            var index = treeRepositoryHeadersCollection.TreeRepositoryHeaders.FindIndex(x => x.Guid == treeRepositoryHeader.Guid);
            if (index == null)
            {
                treeRepositoryHeadersCollection.TreeRepositoryHeaders.Add(treeRepositoryHeader);
            }
            else
            {
                treeRepositoryHeadersCollection.TreeRepositoryHeaders[index] = treeRepositoryHeader;
            }

            json = JsonSerializer.Serialize<TreeRepositoryHeadersCollection>(treeRepositoryHeadersCollection);

            File.WriteAllText(_file.FullName, json);

            return 1;
        }
    }
}
