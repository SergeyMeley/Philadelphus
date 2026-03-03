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
    public class JsonPhiladelphusRepositoriesInfrastructureRepository : IPhiladelphusRepositoriesInfrastructureRepository
    {
        private DirectoryInfo _baseDirectory;
        public JsonPhiladelphusRepositoriesInfrastructureRepository(DirectoryInfo baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }
        public InfrastructureEntityGroups EntityGroup => InfrastructureEntityGroups.PhiladelphusRepositories;

        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }

        public long DeleteRepository(PhiladelphusRepository item)
        {
            throw new NotImplementedException();
        }

        public long InsertRepository(PhiladelphusRepository item)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PhiladelphusRepository> SelectRepositories()
        {
            //TODO: Временный костыль
            return new List<PhiladelphusRepository>();
            throw new NotImplementedException();
        }

        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids)
        {
            //TODO: Временный костыль
            return new List<PhiladelphusRepository>();
            throw new NotImplementedException();
        }

        public long UpdateRepository(PhiladelphusRepository item)
        {
            throw new NotImplementedException();
        }
    }
}
