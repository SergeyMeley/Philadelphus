using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Factories.Interfaces;
using Serilog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs
{
    /// <summary>
    /// Модель представления для коллекции хранилищ данных.
    /// </summary>
    public class DataStoragesCollectionVM : ViewModelBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IDataStoragesService _dataStoragesService;
        private readonly IOptions<ApplicationSettingsConfig> _applicationSettings;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollection;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollection;
        private readonly IInfrastructureRepositoryFactory _infrastructureRepositoryFactory;

        private DataStorageVM _mainDataStorageVM;
        public DataStorageVM MainDataStorageVM
        {
            get
            {
                return _mainDataStorageVM;
            }
        }

        private ObservableCollection<DataStorageVM>? _dataStoragesVMs = new ObservableCollection<DataStorageVM>();
        public ObservableCollection<DataStorageVM>? DataStoragesVMs 
        { 
            get
            {
                return _dataStoragesVMs;
            }
            set
            {
                _dataStoragesVMs = value;
                SubscribeDataStoragesCollectionChanged();
                RefreshVisibleDataStoragesVMs();
                OnPropertyChanged(nameof(DataStoragesVMs));
            }
        }

        public ObservableCollection<DataStorageVM> VisibleDataStoragesVMs { get; }
            = new ObservableCollection<DataStorageVM>();

        private bool _showHiddenDataStorages;
        public bool ShowHiddenDataStorages
        {
            get
            {
                return _showHiddenDataStorages;
            }
            set
            {
                if (_showHiddenDataStorages == value)
                    return;

                _showHiddenDataStorages = value;
                RefreshVisibleDataStoragesVMs();
                OnPropertyChanged(nameof(ShowHiddenDataStorages));
            }
        }

        public IEnumerable<DataStorageVM>? PhiladelphusRepositoriesDataStorageVMs
        {
            get
            {
                return _dataStoragesVMs.Where(x => x.HasPhiladelphusRepositoriesInfrastructureRepository);
            }
        }

        private DataStorageVM _selectedDataStorageVM;
        public DataStorageVM SelectedDataStorageVM 
        {
            get
            {
                return _selectedDataStorageVM;
            }
            set
            {
                _selectedDataStorageVM = value;
                OnPropertyChanged(nameof(SelectedDataStorageVM));
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataStoragesCollectionVM" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="dataStoragesService">Параметр dataStoragesService.</param>
        /// <param name="applicationSettings">Параметр applicationSettings.</param>
        /// <param name="connectionStringsCollection">Параметр connectionStringsCollection.</param>
        /// <param name="dataStoragesCollection">Параметр dataStoragesCollection.</param>
        /// <param name="infrastructureRepositoryFactory">Параметр infrastructureRepositoryFactory.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public DataStoragesCollectionVM(
            ILogger logger,
            INotificationService notificationService,
            IDataStoragesService dataStoragesService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollection,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollection,
            IInfrastructureRepositoryFactory infrastructureRepositoryFactory)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(dataStoragesService);
            ArgumentNullException.ThrowIfNull(applicationSettings);
            ArgumentNullException.ThrowIfNull(applicationSettings.Value);
            ArgumentNullException.ThrowIfNull(connectionStringsCollection);
            ArgumentNullException.ThrowIfNull(connectionStringsCollection.Value);
            ArgumentNullException.ThrowIfNull(dataStoragesCollection);
            ArgumentNullException.ThrowIfNull(dataStoragesCollection.Value);
            ArgumentNullException.ThrowIfNull(infrastructureRepositoryFactory);

            _logger = logger;
            _notificationService = notificationService;
            _dataStoragesService = dataStoragesService;
            _applicationSettings = applicationSettings;
            _connectionStringsCollection = connectionStringsCollection;
            _dataStoragesCollection = dataStoragesCollection;
            _infrastructureRepositoryFactory = infrastructureRepositoryFactory;

            SubscribeDataStoragesCollectionChanged();
            InitMainDataStorageVM();
            InitDataStorages();
            RefreshVisibleDataStoragesVMs();
        }

        private void SubscribeDataStoragesCollectionChanged()
        {
            if (_dataStoragesVMs == null)
                return;

            _dataStoragesVMs.CollectionChanged -= DataStoragesVMsCollectionChanged;
            _dataStoragesVMs.CollectionChanged += DataStoragesVMsCollectionChanged;
        }

        private void DataStoragesVMsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (DataStorageVM storage in e.OldItems)
                {
                    storage.PropertyChanged -= DataStorageVMPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (DataStorageVM storage in e.NewItems)
                {
                    storage.PropertyChanged += DataStorageVMPropertyChanged;
                }
            }

            RefreshVisibleDataStoragesVMs();
        }

        private void DataStorageVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataStorageVM.IsHidden))
            {
                RefreshVisibleDataStoragesVMs();
            }
        }

        private void RefreshVisibleDataStoragesVMs()
        {
            VisibleDataStoragesVMs.Clear();
            if (_dataStoragesVMs == null)
                return;

            foreach (var storage in _dataStoragesVMs.Where(x => ShowHiddenDataStorages || x.IsHidden == false))
            {
                VisibleDataStoragesVMs.Add(storage);
            }
            OnPropertyChanged(nameof(VisibleDataStoragesVMs));
        }
        private bool InitMainDataStorageVM()
        {
            var path = _applicationSettings.Value.MainDataStorage;

            if (path == null)
                throw new Exception();

            if (path.Exists == false)
            {
                path.Create();
            }

            var mainDataStorageModel = _dataStoragesService.CreateMainDataStorageModel(path);
            
            _mainDataStorageVM = new DataStorageVM(mainDataStorageModel);
            _dataStoragesVMs.Add(_mainDataStorageVM);
            
            return true;
        }

        private bool InitDataStorages()
        {
            var models = _dataStoragesService.GetStoragesModels((csc, type, group) =>
                        {
                            csc.ConnectionStrings.TryGetValue(group, out var cs);
                            return _infrastructureRepositoryFactory.Create(type, group, cs);
                        });

            foreach (var model in models)
            {
                if (_dataStoragesVMs?.Any(x => x.Model?.Uuid == model.Uuid) == false)
                {
                    _dataStoragesVMs.Add(new DataStorageVM(model));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return true;
        }

        /// <summary>
        /// Выполняет операцию Dispose.
        /// </summary>
        public void Dispose()
        {
            foreach (var vm in DataStoragesVMs)
                vm.Dispose();
            DataStoragesVMs.Clear();
        }
    }
}
