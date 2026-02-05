using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Json.Repositories
{
    public class JsonTreeRepositoriesInfrastructureRepository : ITreeRepositoriesInfrastructureRepository
    {
        private DirectoryInfo _baseDirectory;
        public JsonTreeRepositoriesInfrastructureRepository(DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }
        public InfrastructureEntityGroups EntityGroup => InfrastructureEntityGroups.TreeRepositories;

        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }

        public long DeleteRepository(TreeRepository item)
        {
            throw new NotImplementedException();
        }

        public long InsertRepository(TreeRepository item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRepository> SelectRepositories()
        {
            //TODO: Временный костыль
            return new List<TreeRepository>();
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRepository> SelectRepositories(Guid[] uuids)
        {
            //TODO: Временный костыль
            return new List<TreeRepository>();
            throw new NotImplementedException();
        }

        public long UpdateRepository(TreeRepository item)
        {
            throw new NotImplementedException();
        }
    }
}
