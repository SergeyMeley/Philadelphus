using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Factories
{
    internal static class InfrastructureFactory
    {
        internal static IMainEntitiesRepository CreateMainEntitiesRepositoriesFactory(InfrastructureRepositoryTypes repositoryType)
        {
            IMainEntitiesRepository mainEntitiesRepository;
            switch (repositoryType)
            {
                case InfrastructureRepositoryTypes.WindowsDirectory:
                    mainEntitiesRepository = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
                    break;
                case InfrastructureRepositoryTypes.PostgreSql:
                    mainEntitiesRepository = new PostgreRepository.Repositories.MainEntityRepository();
                    break;
                case InfrastructureRepositoryTypes.MongoDb:
                    mainEntitiesRepository = new MongoRepository.Repositories.MainEntitуRepository();
                    break;
                default:
                    mainEntitiesRepository = null;
                    break;
            }
            return mainEntitiesRepository;
        }
    }
}
