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
        internal static IMainEntitiesInfrastructureRepository GetMainEntitiesInfrastructure(InfrastructureTypes repositoryType)
        {
            IMainEntitiesInfrastructureRepository mainEntitiesRepository = null;
            switch (repositoryType)
            {
                case InfrastructureTypes.WindowsDirectory:
                    //mainEntitiesRepository = new WindowsFileSystemRepository.Repositories.WindowsMainEntityRepository();
                    break;
                case InfrastructureTypes.PostgreSql:
                    mainEntitiesRepository = new PostgreEfRepository.Repositories.MainEntitiesInfrastructureRepository();
                    break;
                case InfrastructureTypes.MongoDb:
                    //mainEntitiesRepository = new MongoRepository.Repositories.MongoMainEntitуInfrastructure();
                    break;
                default:
                    throw new Exception($"Неизвестная инфраструктура ({repositoryType.ToString()}), измените настройки и повторите попытку.");
                    break;
            }
            return mainEntitiesRepository;
        }
    }
}
