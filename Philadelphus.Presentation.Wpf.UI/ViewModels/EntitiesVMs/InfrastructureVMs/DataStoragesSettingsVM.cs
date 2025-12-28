using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Config;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Models.StorageConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs
{
    public class DataStoragesSettingsVM : ViewModelBase
    {
        private readonly ILogger<DataStoragesSettingsVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryCollectionService _treeRepositoryCollectionService;
        private readonly StorageConfigService _storageConfigService;

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
            ITreeRepositoryCollectionService treeRepositoryCollectionService,
            StorageConfigService storageConfigService,   //TODO: Заменить на новый сервис
            IOptions<ApplicationSettings> options)
        {
            _logger = logger;
            _notificationService = notificationService;
            _treeRepositoryCollectionService = treeRepositoryCollectionService;
            _storageConfigService = storageConfigService;

            InitMainDataStorageVM(options.Value.ConfigsDirectory);
            InitDataStorages();
        }
        private bool InitMainDataStorageVM(DirectoryInfo configsDirectory)
        {
            var mainDataStorageModel = _treeRepositoryCollectionService.CreateMainDataStorageModel(configsDirectory);
            _mainDataStorageVM = new DataStorageVM(mainDataStorageModel);
            _dataStorageVMs.Add(_mainDataStorageVM);
            return true;
        }
        private bool InitDataStorages()
        {
            var service = new StorageConfigService();
            service.LoadConfig();
            var models = service.GetAllStorageModels();
            foreach (var model in models)
            {
                if (model != null)
                {
                    if (_dataStorageVMs.FirstOrDefault(x => x.Model.Guid == model.Guid) == null)
                    {
                        _dataStorageVMs.Add(new DataStorageVM(model));
                    }
                }
            }
            return true;
        }
    }
}
