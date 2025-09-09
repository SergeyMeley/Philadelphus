using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.PostgreEfRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Infrastructure
{
    public class DataStorageModel : IDataStorageModel
    {
        public Guid Guid { get; }
        public string Name { get; }
        public string Description { get; }
        public IDataStorageInfrastructureRepository DataStorageInfrastructureRepositoryRepository { get; set; }
        public ITreeRepositoryHeadersInfrastructureRepository TreeRepositoryHeadersInfrastructureRepository { get; set; }
        public IMainEntitiesInfrastructureRepository MainEntitiesInfrastructureRepository { get; set; }
        public bool IsAvailable {
            get
            {
                if (DataStorageInfrastructureRepositoryRepository.CheckAvailability() == false)
                    return false;
                if (TreeRepositoryHeadersInfrastructureRepository.CheckAvailability() == false)
                    return false;
                if (MainEntitiesInfrastructureRepository.CheckAvailability() == false)
                    return false;
                return true;
            }
        }
        
        internal DataStorageModel(Guid guid, string name, string description)
        {
            Guid = guid;
            Name = name;
            Description = description;
        }
    }
}
