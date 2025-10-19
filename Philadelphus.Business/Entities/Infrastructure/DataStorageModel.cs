using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.PostgreEfRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
            get => (IMainEntitiesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.MainEntitiesInfrastructureRepository];
        }
        private bool _isAvailable = false;
        public bool IsAvailable { get => _isAvailable; }

        private DateTime _lastCheckTime;
        public DateTime LastCheckTime { get => _lastCheckTime; }
        internal DataStorageModel(Guid guid, string name, string description, InfrastructureTypes infrastructureType)
        {
            Guid = guid;
            Name = name;
            Description = description;
            InfrastructureType = infrastructureType;
            CheckAvailable();
        }
        public bool StartAvailableAutoChecking()
        {
            System.Timers.Timer timer = new System.Timers.Timer(10000);
            timer.Elapsed += CheckAvailable;
            timer.AutoReset = true;
            timer.Enabled = true;
            return true;
        }
        public bool CheckAvailable()
        {
            var result = true;
            foreach (var item in InfrastructureRepositories)
            {
                if (item.Value.CheckAvailability() == false)
                {
                    result = false;
                    break;
                }
                    
            }
            _lastCheckTime = DateTime.Now;
            _isAvailable = result;
            return _isAvailable;
        }
        private void CheckAvailable(Object source, ElapsedEventArgs e)
        {
            CheckAvailable();
        }
    }
}
