using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Philadelphus.Infrastructure.Persistence.Json.Repositories
{
    public class JsonTreeRepositoryHeadersCollectionInfrastructureRepository : ITreeRepositoryHeadersCollectionInfrastructureRepository
    {
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.TreeRepositoryHeadersCollection; }

        private FileInfo _file;
        public JsonTreeRepositoryHeadersCollectionInfrastructureRepository(
            FileInfo repositoryHeadersConfigFullPath)
        {
            if (repositoryHeadersConfigFullPath == null || repositoryHeadersConfigFullPath.Exists == false)
                throw new Exception($"Некорректный путь к настроечному файлу: '{repositoryHeadersConfigFullPath}'");
            _file = repositoryHeadersConfigFullPath ;
        }
        public bool CheckAvailability()
        {
            if (_file == null)
                return false;
            if (_file.Exists == false)
                return false;
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
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            result = JsonSerializer.Deserialize<TreeRepositoryHeadersCollection>(json, options).TreeRepositoryHeaders;

            if (result == null)
                throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");

            return result;
        }

        public long UpdateRepository(TreeRepositoryHeader treeRepositoryHeader) // TODO: Тех. долг по задаче #276
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            var json = File.ReadAllText(_file.FullName);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            var root = JsonSerializer.Deserialize<JsonElement>(json);

            TreeRepositoryHeadersCollection treeRepositoryHeadersCollection = null;

            if (root.TryGetProperty("TreeRepositoryHeadersCollection", out var collectionNode))
            {
                treeRepositoryHeadersCollection = JsonSerializer.Deserialize<TreeRepositoryHeadersCollection>(collectionNode, options);
            }

            if (treeRepositoryHeadersCollection == null)
                throw new InvalidOperationException("Ошибка десериализации конфигурационного файла");

            var index = treeRepositoryHeadersCollection.TreeRepositoryHeaders.FindIndex(x => x.Uuid == treeRepositoryHeader.Uuid);
            if (index == null || index == -1)
            {
                treeRepositoryHeadersCollection.TreeRepositoryHeaders.Add(treeRepositoryHeader);
            }
            else
            {
                treeRepositoryHeadersCollection.TreeRepositoryHeaders[index] = treeRepositoryHeader;
            }

            var wrapper = new
            {
                TreeRepositoryHeadersCollection = treeRepositoryHeadersCollection
            };
            json = JsonSerializer.Serialize(wrapper, options);

            File.WriteAllText(_file.FullName, json);

            return 1;
        }
    }
}
