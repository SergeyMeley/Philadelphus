using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Serilog;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
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
                OnPropertyChanged(nameof(DataStoragesVMs));
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

        public DataStoragesCollectionVM(
            ILogger logger,
            INotificationService notificationService,
            IDataStoragesService dataStoragesService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollection,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollection,
            IInfrastructureRepositoryFactory infrastructureRepositoryFactory)
        {
            _logger = logger;
            _notificationService = notificationService;
            _dataStoragesService = dataStoragesService;
            _applicationSettings = applicationSettings;
            _connectionStringsCollection = connectionStringsCollection;
            _dataStoragesCollection = dataStoragesCollection;
            _infrastructureRepositoryFactory = infrastructureRepositoryFactory;

            InitMainDataStorageVM(applicationSettings.Value);
            InitDataStorages();
        }
        private bool InitMainDataStorageVM(ApplicationSettingsConfig applicationSettings)
        {
            if (applicationSettings.MainDataStorage == null)
                throw new Exception(); 
            var mainDataStorageModel = _dataStoragesService.CreateMainDataStorageModel(applicationSettings.MainDataStorage);
            _mainDataStorageVM = new DataStorageVM(mainDataStorageModel);
            _dataStoragesVMs.Add(_mainDataStorageVM);
            return true;
        }

        private bool InitDataStorages()
        {
            var models = _dataStoragesService.GetStoragesModels((cs, type, group) => _infrastructureRepositoryFactory.Create(type, group, cs.ConnectionString));

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

        public void Dispose()
        {
            foreach (var vm in DataStoragesVMs)
                vm.Dispose();
            DataStoragesVMs.Clear();
        }
    }
}
