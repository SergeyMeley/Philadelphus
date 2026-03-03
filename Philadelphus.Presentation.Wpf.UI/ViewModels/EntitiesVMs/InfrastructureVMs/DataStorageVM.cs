using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using System.Timers;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
    public class DataStorageVM : ViewModelBase, IDisposable
    {
        private System.Timers.Timer? _timer;
        private IDataStorageModel? _model;
        public IDataStorageModel? Model
        { 
            get
            {
                return _model;
            }
        }
        public Guid Uuid 
        { 
            get
            {
                return _model.Uuid;
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
                OnPropertyChanged(nameof(Name));
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
                OnPropertyChanged(nameof(Description));
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

        public bool HasPhiladelphusRepositoriesInfrastructureRepository
        {
            get
            {
                if (_model.PhiladelphusRepositoriesInfrastructureRepository == null)
                    return false;
                return true;
            }
        }
        public bool HasMainEntitiesInfrastructureRepository
        {
            get
            {
                if (_model.PhiladelphusRepositoryMembersInfrastructureRepository == null)
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
            _model.StartAvailableAutoChecking(interval: 20);
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += (s, e) => 
            {
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(LastCheckTime));
            }
            ;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            Model?.StopAvailableAutoChecking();
            _timer = null;
        }
    }
}
