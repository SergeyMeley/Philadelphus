using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
    public class DataStoragesSettingsVM : ViewModelBase
    {
        private readonly ILogger<DataStoragesSettingsVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly IDataStoragesService _dataStoragesService;

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

        public DataStoragesSettingsVM(
            ILogger<DataStoragesSettingsVM> logger,
            INotificationService notificationService,
            IDataStoragesService dataStoragesService,
            IOptions<ApplicationSettings> applicationSettings,
            IOptions<ConnectionStringsCollection> connectionStringsCollection,
            IOptions<DataStoragesCollection> dataStoragesCollection)
        {
            _logger = logger;
            _notificationService = notificationService;
            _dataStoragesService = dataStoragesService;

            InitMainDataStorageVM(applicationSettings.Value);
            //InitDataStorages(connectionStringsCollection.Value);
            InitDataStorages(dataStoragesCollection.Value, connectionStringsCollection.Value);
        }
        private bool InitMainDataStorageVM(ApplicationSettings applicationSettings)
        {
            var mainDataStorageModel = _dataStoragesService.CreateMainDataStorageModel(applicationSettings.StoragesConfigFullPath, applicationSettings.RepositoryHeadersConfigFullPath);
            _mainDataStorageVM = new DataStorageVM(mainDataStorageModel);
            _dataStorageVMs.Add(_mainDataStorageVM);
            return true;
        }
        private bool InitDataStorages(ConnectionStringsCollection connectionStrings)
        {
            
            var models = _dataStoragesService.GetStoragesModels(connectionStrings);
            foreach (var model in models)
            {
                if (model != null)
                {
                    if (_dataStorageVMs.FirstOrDefault(x => x.Model.Uuid == model.Uuid) == null)
                    {
                        _dataStorageVMs.Add(new DataStorageVM(model));
                    }
                }
            }
            return true;
        }

        private bool InitDataStorages(DataStoragesCollection dataStoragesCollection, ConnectionStringsCollection connectionStrings)
        {
            foreach (var entity in dataStoragesCollection.DataStorages)
            {
                if (entity != null)
                {
                    var connectionString = connectionStrings.ConnectionStringContainers.SingleOrDefault(x => x.Uuid == entity.Uuid).ConnectionString;
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
            return true;
        }
    }
}
