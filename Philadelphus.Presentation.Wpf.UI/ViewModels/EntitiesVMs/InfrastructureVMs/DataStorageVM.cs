using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using System.Timers;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления для хранилища данных.
    /// </summary>
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
                return _model.HasPhiladelphusRepositoriesInfrastructureRepository;
            }
        }
        public bool HasMainEntitiesInfrastructureRepository
        {
            get
            {
                return _model.HasShrubMembersInfrastructureRepository;
            }
        }
        public bool HasReportsInfrastructureRepository
        {
            get
            {
                return _model.HasReportsInfrastructureRepository;
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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataStorageVM" />.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public DataStorageVM(IDataStorageModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _model = model;
            StartCheckingStorage();
        }
        private void StartCheckingStorage()
        {
            _model.StartAvailableAutoChecking(interval: 60);
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += (s, e) => 
            {
                OnPropertyChanged(nameof(IsAvailable));
                OnPropertyChanged(nameof(LastCheckTime));
            };
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            Model?.StopAvailableAutoChecking();
            _timer = null;
        }
    }
}
