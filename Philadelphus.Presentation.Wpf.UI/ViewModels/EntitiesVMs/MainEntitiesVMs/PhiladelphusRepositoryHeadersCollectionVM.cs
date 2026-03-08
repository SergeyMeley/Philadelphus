using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class PhiladelphusRepositoryHeadersCollectionVM : ViewModelBase  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly ILogger<PhiladelphusRepositoryHeadersCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly IPhiladelphusRepositoryCollectionService _service;
        private readonly DataStoragesCollectionVM _dataStoragesSettingsVM;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _PhiladelphusRepositoryHeadersCollectionConfig;

        private ObservableCollection<PhiladelphusRepositoryHeaderVM> _PhiladelphusRepositoryHeadersVMs;
        public ObservableCollection<PhiladelphusRepositoryHeaderVM> PhiladelphusRepositoryHeadersVMs
        {
            get
            {
                return _PhiladelphusRepositoryHeadersVMs;
            }
            private set
            {
                if (_PhiladelphusRepositoryHeadersVMs != value)
                {
                    _PhiladelphusRepositoryHeadersVMs = value;
                    OnPropertyChanged();
                }
            }
        }

        public CollectionViewSource FavoritePhiladelphusRepositoryHeadersVMs { get; }
        public CollectionViewSource LastPhiladelphusRepositoryHeadersVMs { get; }

        private PhiladelphusRepositoryHeaderVM _selectedPhiladelphusRepositoryHeaderVM;
        public PhiladelphusRepositoryHeaderVM SelectedPhiladelphusRepositoryHeaderVM 
        {
            get
            { 
                return _selectedPhiladelphusRepositoryHeaderVM; 
            }
            set
            {
                _selectedPhiladelphusRepositoryHeaderVM = value;
                if (_selectedPhiladelphusRepositoryHeaderVM != null)
                    CheckPhiladelphusRepositoryAvailable(_selectedPhiladelphusRepositoryHeaderVM);
                OnPropertyChanged(nameof(SelectedPhiladelphusRepositoryHeaderVM));
            }
        }

        public Predicate<PhiladelphusRepositoryHeaderVM> CheckPhiladelphusRepositoryAvailableAction;

        private Action _updatePhiladelphusRepositoryHeaders
        {
            get
            {
                return new Action(() =>
                {
                    OnPropertyChanged(nameof(PhiladelphusRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(FavoritePhiladelphusRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(LastPhiladelphusRepositoryHeadersVMs));
                    FavoritePhiladelphusRepositoryHeadersVMs.View.Refresh();
                    LastPhiladelphusRepositoryHeadersVMs.View.Refresh();
                });
            }
        }
        public PhiladelphusRepositoryHeadersCollectionVM(
            ILogger<PhiladelphusRepositoryHeadersCollectionVM> logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService service,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IConfigurationService configurationService,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> philadelphusRepositoryHeadersCollectionConfig)
        {
            _logger = logger;
            _notificationService = notificationService;
            _service = service;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _configurationService = configurationService;
            _appConfig = appConfig;
            _PhiladelphusRepositoryHeadersCollectionConfig = philadelphusRepositoryHeadersCollectionConfig;

            _PhiladelphusRepositoryHeadersVMs = new ObservableCollection<PhiladelphusRepositoryHeaderVM>();
            LoadPhiladelphusRepositoryHeadersVMs(philadelphusRepositoryHeadersCollectionConfig);

            FavoritePhiladelphusRepositoryHeadersVMs = new CollectionViewSource { Source = PhiladelphusRepositoryHeadersVMs };
            FavoritePhiladelphusRepositoryHeadersVMs.Filter += (s, e) =>
            {
                var item = e.Item as PhiladelphusRepositoryHeaderVM;
                if (item == null) 
                { 
                    e.Accepted = false; 
                    return; 
                }
                e.Accepted = item.IsFavorite;
            };

            LastPhiladelphusRepositoryHeadersVMs = new CollectionViewSource { Source = PhiladelphusRepositoryHeadersVMs };
            LastPhiladelphusRepositoryHeadersVMs.Filter += (s, e) =>
            {
                var item = e.Item as PhiladelphusRepositoryHeaderVM;
                if (item == null)
                {
                    e.Accepted = false;
                    return;
                }
                e.Accepted = DateTime.UtcNow - item.LastOpening <= TimeSpan.FromDays(90);
            };

            PhiladelphusRepositoryHeadersVMs.CollectionChanged += (s, e) =>
            {
                FavoritePhiladelphusRepositoryHeadersVMs.View.Refresh();
                LastPhiladelphusRepositoryHeadersVMs.View.Refresh();
            };
        }
        public bool CheckPhiladelphusRepositoryAvailable(PhiladelphusRepositoryHeaderVM header)
        {
            if (header == null)
                return false;
            if (CheckPhiladelphusRepositoryAvailableAction == null)
                return false;
            CheckPhiladelphusRepositoryAvailableAction.Invoke(header);
            return header.IsPhiladelphusRepositoryAvailable;
        }
        private ObservableCollection<PhiladelphusRepositoryHeaderVM> LoadPhiladelphusRepositoryHeadersVMs(IOptions<PhiladelphusRepositoryHeadersCollectionConfig> config) 
        {
            _PhiladelphusRepositoryHeadersVMs.Clear();
            var headers = config.Value.PhiladelphusRepositoryHeaders;
            if (headers == null)
                return null;
            headers.OrderByDescending(x => x.LastOpening);

            foreach (var header in headers)
            {
                var vm = new PhiladelphusRepositoryHeaderVM(header.ToModel(), _service, _dataStoragesSettingsVM.MainDataStorageVM, _updatePhiladelphusRepositoryHeaders, _configurationService, _appConfig, _PhiladelphusRepositoryHeadersCollectionConfig);
                CheckPhiladelphusRepositoryAvailable(vm);
                _PhiladelphusRepositoryHeadersVMs.Add(vm);
            }
            return PhiladelphusRepositoryHeadersVMs;
        }
        internal PhiladelphusRepositoryHeaderVM  AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(PhiladelphusRepositoryVM PhiladelphusRepositoryVM)
        {
            var header = _service.CreatePhiladelphusRepositoryHeaderFromPhiladelphusRepository(PhiladelphusRepositoryVM.Model);

            _PhiladelphusRepositoryHeadersCollectionConfig.Value.PhiladelphusRepositoryHeaders.Add(header.ToDbEntity());
            _configurationService.UpdateConfigFile(_appConfig.Value.RepositoryHeadersConfigFullPath, _PhiladelphusRepositoryHeadersCollectionConfig);

            var result = new PhiladelphusRepositoryHeaderVM(header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updatePhiladelphusRepositoryHeaders, _configurationService, _appConfig, _PhiladelphusRepositoryHeadersCollectionConfig);
            PhiladelphusRepositoryHeadersVMs.Add(result);
            return result;
        }
    }
}
