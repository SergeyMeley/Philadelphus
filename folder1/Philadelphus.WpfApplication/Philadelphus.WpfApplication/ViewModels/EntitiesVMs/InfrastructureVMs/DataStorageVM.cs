using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs
{
    public class DataStorageVM : ViewModelBase
    {
        private IDataStorageModel? _model;
        public IDataStorageModel? Model
        { 
            get
            {
                return _model;
            }
        }
        public Guid Guid 
        { 
            get
            {
                return _model.Guid;
            }
        }
        public string Name 
        {
            get
            {
                return _model.Name;
            }
            set
            {
                _model.Name = value;
            }
        }
        public string Description
        {
            get
            {
                return _model.Description;
            }
            set
            {
                _model.Description = value;
            }
        }
        public InfrastructureTypes InfrastructureType
        {
            get
            {
                return _model.InfrastructureType;
            }
        }
        public bool HasInfrastructureRepositories
        {
            get
            {
                if (_model.InfrastructureRepositories == null || _model.InfrastructureRepositories.Count() == 0)
                    return false;
                return true;
            }
        }
        public bool HasDataStoragesCollectionInfrastructureRepository
        {
            get
            {
                if (_model.DataStoragesCollectionInfrastructureRepository == null)
                    return false;
                return true;
            }
        }
        public bool HasTreeRepositoryHeadersCollectionInfrastructureRepository
        {
            get
            {
                if (_model.TreeRepositoryHeadersCollectionInfrastructureRepository == null)
                    return false;
                return true;
            }
        }
        public bool HasTreeRepositoriesInfrastructureRepository
        {
            get
            {
                if (_model.TreeRepositoriesInfrastructureRepository == null)
                    return false;
                return true;
            }
        }
        public bool HasMainEntitiesInfrastructureRepository
        {
            get
            {
                if (_model.MainEntitiesInfrastructureRepository == null)
                    return false;
                return true;
            }
        }
        public bool? IsAvailable 
        {
            get
            {
                return _model.IsAvailable;
            }
        }
        public DateTime? LastCheckTime 
        {
            get
            {
                return _model.LastCheckTime;
            }
        }
        public DataStorageVM(IDataStorageModel model)
        {
            if (model == null)
                throw new ArgumentNullException();
            _model = model;
            StartCheckingStorage();
        }
        private void StartCheckingStorage()
        {
            _model.StartAvailableAutoChecking();
            System.Timers.Timer timer = new System.Timers.Timer(100);
            timer.Elapsed += CheckStorage;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        private void CheckStorage()
        {
            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(Model.IsAvailable));
            OnPropertyChanged(nameof(Model.LastCheckTime));
        }
        private void CheckStorage(object source, ElapsedEventArgs e)
        {
            CheckStorage();
        }
    }
}
