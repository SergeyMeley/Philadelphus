using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
    public class DataStoragesCollectionVM : ViewModelBase, IDisposable
    {
        private readonly ILogger<DataStoragesCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly IDataStoragesService _dataStoragesService;
        private readonly IOptions<ApplicationSettingsConfig> _applicationSettings;
        private readonly IOptions<ConnectionStringsCollectionConfig> _connectionStringsCollection;
        private readonly IOptions<DataStoragesCollectionConfig> _dataStoragesCollection;

        private DataStorageVM _mainDataStorageVM;
        public DataStorageVM MainDataStorageVM
        {
            get
            {
                return _mainDataStorageVM;
            }
        }

        private ObservableCollection<DataStorageVM>? _dataStorageVMs = new ObservableCollection<DataStorageVM>();
        public ObservableCollection<DataStorageVM>? DataStorageVMs 
        { 
            get
            {
                return _dataStorageVMs;
            }
            set
            {
                _dataStorageVMs = value;
                OnPropertyChanged(nameof(DataStorageVMs));
            }
        }
        public IEnumerable<DataStorageVM>? TreeRepositoriesDataStorageVMs
        {
            get
            {
                return _dataStorageVMs.Where(x => x.HasTreeRepositoriesInfrastructureRepository);
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
            ILogger<DataStoragesCollectionVM> logger,
            INotificationService notificationService,
            IDataStoragesService dataStoragesService,
            IOptions<ApplicationSettingsConfig> applicationSettings,
            IOptions<ConnectionStringsCollectionConfig> connectionStringsCollection,
            IOptions<DataStoragesCollectionConfig> dataStoragesCollection)
        {
            _logger = logger;
            _notificationService = notificationService;
            _dataStoragesService = dataStoragesService;
            _applicationSettings = applicationSettings;
            _connectionStringsCollection = connectionStringsCollection;
            _dataStoragesCollection = dataStoragesCollection;

            InitMainDataStorageVM(applicationSettings.Value);
            //InitDataStorages(connectionStringsCollection.Value);
            InitDataStorages(dataStoragesCollection.Value, connectionStringsCollection.Value);
        }
        private bool InitMainDataStorageVM(ApplicationSettingsConfig applicationSettings)
        {
            if (applicationSettings.TryGetConfigFileFullPath<ConnectionStringsCollectionConfig>(out var config1)
                && applicationSettings.TryGetConfigFileFullPath<TreeRepositoryHeadersCollectionConfig>(out var config2))
            {
                var mainDataStorageModel = _dataStoragesService.CreateMainDataStorageModel(config1, config2);
                _mainDataStorageVM = new DataStorageVM(mainDataStorageModel);
                _dataStorageVMs.Add(_mainDataStorageVM);
                return true;
            }
            else
            {
                throw new Exception();
            }
        }

        private bool InitDataStorages(DataStoragesCollectionConfig dataStoragesCollection, ConnectionStringsCollectionConfig connectionStrings)
        {
            foreach (var entity in dataStoragesCollection.DataStorages)
            {
                if (entity != null)
                {
                    var connectionString = connectionStrings.ConnectionStringContainers.SingleOrDefault(x => x.Uuid == entity.Uuid)?.ConnectionString;

                    if (connectionString == null)
                    {
                        _logger.LogError($"Не найдена строка подключения для хранилища {entity.Name}");
                        MessageBox.Show($"Не найдена строка подключения для хранилища {entity.Name}");
                    }
                    else
                    {
                        var model = entity.ToModel(connectionString);

                        if (_dataStorageVMs?.Any(x => x.Model?.Uuid == model.Uuid) == false)
                        {
                            _dataStorageVMs.Add(new DataStorageVM(model));
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
            return true;
        }

        public void Dispose()
        {
            foreach (var vm in DataStorageVMs)
                vm.Dispose();
            DataStorageVMs.Clear();
        }
    }
}
