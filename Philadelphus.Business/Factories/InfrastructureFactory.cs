using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
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
        internal static IMainEntitiesRepository CreateMainEntitiesRepositoriesFactory(InftastructureRepositoryTypes repositoryType)
        {
            IMainEntitiesRepository mainEntitiesRepository;
            switch (repositoryType)
            {
                case InftastructureRepositoryTypes.WindowsDirectory:
                    mainEntitiesRepository = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
                    break;
                case InftastructureRepositoryTypes.PostgreSql:
                    mainEntitiesRepository = new PostgreRepository.Repositories.MainEntityRepository();
                    break;
                case InftastructureRepositoryTypes.MongoDb:
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
