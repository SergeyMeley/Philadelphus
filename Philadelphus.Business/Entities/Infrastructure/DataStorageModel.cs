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
        public string Name { get; set; }
        public string Description { get; set; }
        public InfrastructureTypes InfrastructureType { get; set; }

        private Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> _infrastructureRepositories;
        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories 
        { 
            get
            {
                if (_isDisabled)
                    return null;
                return _infrastructureRepositories;
            }
            internal set
            {
                if (_isDisabled)
                    return;
                _infrastructureRepositories = value;
            }
        }
        public IDataStoragesCollectionInfrastructureRepository DataStoragesCollectionInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                return (IDataStoragesCollectionInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.DataStoragesCollection];
            }
        }
        public ITreeRepositoryHeadersCollectionInfrastructureRepository TreeRepositoryHeadersCollectionInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                return (ITreeRepositoryHeadersCollectionInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.TreeRepositoryHeadersCollection];
            }
        }
        public ITreeRepositoriesInfrastructureRepository TreeRepositoriesInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                return (ITreeRepositoriesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.TreeRepositories];
            }
        }
        public IMainEntitiesInfrastructureRepository MainEntitiesInfrastructureRepository
        {
            get
            {
                if (_isDisabled)
                    return null;
                return (IMainEntitiesInfrastructureRepository)InfrastructureRepositories[InfrastructureEntityGroups.MainEntities];
            }
        }
        private bool _isAvailable = false;
        public bool IsAvailable { get => _isAvailable; }

        private bool _isDisabled;
        public bool IsDisabled { get => _isDisabled; set => _isDisabled = value; }

        private DateTime _lastCheckTime;
        public DateTime LastCheckTime { get => _lastCheckTime; }
        internal DataStorageModel(Guid guid, string name, string description, InfrastructureTypes infrastructureType, bool isDisabled)
        {
            Guid = guid;
            Name = name;
            Description = description;
            InfrastructureType = infrastructureType;
            IsDisabled = isDisabled;
            if (IsDisabled == false)
            {
                InfrastructureRepositories = new Dictionary<InfrastructureEntityGroups, IInfrastructureRepository>();
            }
            CheckAvailable();
        }
        public bool StartAvailableAutoChecking()
        {
            if (_isDisabled)
                return false;
            System.Timers.Timer timer = new System.Timers.Timer(10000);
            timer.Elapsed += CheckAvailable;
            timer.AutoReset = true;
            timer.Enabled = true;
            return true;
        }
        public bool CheckAvailable()
        {
            if (_isDisabled)
                return false;
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
