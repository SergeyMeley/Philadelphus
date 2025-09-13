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
        public InfrastructureTypes InfrastructureType { get; set; }
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories { get; internal set; } = new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();
        public IDataStoragesInfrastructureRepository DataStorageInfrastructureRepositoryRepository 
        { 
            get => (IDataStoragesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.DataStoragesInfrastructureRepository];
        }
        public ITreeRepositoriesInfrastructureRepository TreeRepositoryHeadersInfrastructureRepository
        {
            get => (ITreeRepositoriesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.TreeRepositoriesInfrastructureRepository];
        }
        public IMainEntitiesInfrastructureRepository MainEntitiesInfrastructureRepository
        {
            get => (IMainEntitiesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.TreeRepositoriesInfrastructureRepository];
        }
        public bool IsAvailable {
            get
            {
                foreach (var item in InfrastructureRepositories)
                {
                    if (item.Value.CheckAvailability() == false)
                        return false;
                }
                return true;
            }
        }
        internal DataStorageModel(Guid guid, string name, string description, InfrastructureTypes infrastructureType)
        {
            Guid = guid;
            Name = name;
            Description = description;
            InfrastructureType = infrastructureType;
        }
    }
}
