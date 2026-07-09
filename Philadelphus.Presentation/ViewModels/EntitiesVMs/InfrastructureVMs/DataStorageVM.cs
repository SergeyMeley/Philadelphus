using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Presentation.Helpers;
using System.Timers;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления для хранилища данных.
    /// </summary>
    public class DataStorageVM : ViewModelBase, IDisposable
    {
        private System.Timers.Timer? _timer;
        private IDataStorageModel? _model;
        private readonly INotificationService? _notificationService;
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
                if (TryRejectMainDataStorageSettingsChange(nameof(Name)))
                    return;

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
                if (TryRejectMainDataStorageSettingsChange(nameof(Description)))
                    return;

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
        public bool IsMainDataStorage
        {
            get
            {
                return _model.IsMainDataStorage;
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
        public bool IsHidden
        {
            get
            {
                return _model.IsHidden;
            }
            set
            {
                if (TryRejectMainDataStorageSettingsChange(nameof(IsHidden), nameof(HeaderOpacity)))
                    return;

                _model.IsHidden = value;
                OnPropertyChanged(nameof(IsHidden));
                OnPropertyChanged(nameof(HeaderOpacity));
            }
        }
        public bool IsDisabled
        {
            get
            {
                return _model.IsDisabled;
            }
            set
            {
                if (TryRejectMainDataStorageSettingsChange(nameof(IsDisabled)))
                    return;

                _model.IsDisabled = value;
                OnPropertyChanged(nameof(IsDisabled));
            }
        }
        public double HeaderOpacity
        {
            get
            {
                return HiddenElementPresentationHelper.GetOpacity(IsHidden);
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
        public DataStorageVM(IDataStorageModel model, INotificationService? notificationService = null)
        {
            ArgumentNullException.ThrowIfNull(model);

            _model = model;
            _notificationService = notificationService;
            StartCheckingStorage();
        }
        private bool TryRejectMainDataStorageSettingsChange(params string[] propertyNames)
        {
            if (IsMainDataStorage == false)
                return false;

            _notificationService?.SendTextMessage<DataStorageVM>(
                "Настройки основного хранилища нельзя изменять.",
                NotificationCriticalLevelModel.Warning);

            foreach (var propertyName in propertyNames)
                OnPropertyChanged(propertyName);

            return true;
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
