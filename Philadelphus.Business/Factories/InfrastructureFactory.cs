using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Factories
{
    internal static class InfrastructureFactory
    {
        internal static IMainEntitiesInfrastructure GetMainEntitiesInfrastructure(InfrastructureTypes repositoryType)
        {
            IMainEntitiesInfrastructure mainEntitiesRepository;
            switch (repositoryType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    mainEntitiesRepository = new WindowsFileSystemRepository.Repositories.WindowsMainEntityRepository();
                    break;
                case InfrastructureTypes.PostgreSql:
                    mainEntitiesRepository = new PostgreInfrastructure.Repositories.PostgreMainEntityInfrastructure();
                    break;
                case InfrastructureTypes.MongoDb:
                    mainEntitiesRepository = new MongoRepository.Repositories.MongoMainEntitуInfrastructure();
                    break;
                default:
                    throw new Exception($"Неизвестная инфраструктура ({repositoryType.ToString()}), измените настройки и повторите попытку.");
                    break;
            }
            return mainEntitiesRepository;
        }
    }
}
