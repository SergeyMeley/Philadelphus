using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Factories
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
                case InfrastructureTypes.PostgreSqlEf:
                    //mainEntitiesRepository = new PostgreEfRepository.Repositories.PostgreEfMainEntitiesInfrastructureRepository();
                    break;
                case InfrastructureTypes.MongoDbAdo:
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
