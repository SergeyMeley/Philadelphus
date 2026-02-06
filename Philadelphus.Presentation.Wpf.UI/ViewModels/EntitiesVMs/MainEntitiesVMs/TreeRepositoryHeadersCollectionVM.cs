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
    public class TreeRepositoryHeadersCollectionVM : ViewModelBase  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly ILogger<TreeRepositoryHeadersCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryCollectionService _service;
        private readonly DataStoragesCollectionVM _dataStoragesSettingsVM;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<TreeRepositoryHeadersCollectionConfig> _treeRepositoryHeadersCollectionConfig;

        private ObservableCollection<TreeRepositoryHeaderVM> _treeRepositoryHeadersVMs;
        public ObservableCollection<TreeRepositoryHeaderVM> TreeRepositoryHeadersVMs
        {
            get
            {
                return _treeRepositoryHeadersVMs;
            }
            private set
            {
                if (_treeRepositoryHeadersVMs != value)
                {
                    _treeRepositoryHeadersVMs = value;
                    OnPropertyChanged();
                }
            }
        }

        public CollectionViewSource FavoriteTreeRepositoryHeadersVMs { get; }
        public CollectionViewSource LastTreeRepositoryHeadersVMs { get; }

        private TreeRepositoryHeaderVM _selectedTreeRepositoryHeaderVM;
        public TreeRepositoryHeaderVM SelectedTreeRepositoryHeaderVM 
        {
            get
            { 
                return _selectedTreeRepositoryHeaderVM; 
            }
            set
            {
                _selectedTreeRepositoryHeaderVM = value;
                if (_selectedTreeRepositoryHeaderVM != null)
                    CheckTreeRepositoryAvailable(_selectedTreeRepositoryHeaderVM);
                OnPropertyChanged(nameof(SelectedTreeRepositoryHeaderVM));
            }
        }

        public Predicate<TreeRepositoryHeaderVM> CheckTreeRepositoryAvailableAction;

        private Action _updateTreeRepositoryHeaders
        {
            get
            {
                return new Action(() =>
                {
                    OnPropertyChanged(nameof(TreeRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(FavoriteTreeRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(LastTreeRepositoryHeadersVMs));
                    FavoriteTreeRepositoryHeadersVMs.View.Refresh();
                    LastTreeRepositoryHeadersVMs.View.Refresh();
                });
            }
        }
        public TreeRepositoryHeadersCollectionVM(
            ILogger<TreeRepositoryHeadersCollectionVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService service,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IConfigurationService configurationService,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<TreeRepositoryHeadersCollectionConfig> treeRepositoryHeadersCollectionConfig)
        {
            _logger = logger;
            _notificationService = notificationService;
            _service = service;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _configurationService = configurationService;
            _appConfig = appConfig;
            _treeRepositoryHeadersCollectionConfig = treeRepositoryHeadersCollectionConfig;

            _treeRepositoryHeadersVMs = new ObservableCollection<TreeRepositoryHeaderVM>();
            LoadTreeRepositoryHeadersVMs();

            FavoriteTreeRepositoryHeadersVMs = new CollectionViewSource { Source = TreeRepositoryHeadersVMs };
            FavoriteTreeRepositoryHeadersVMs.Filter += (s, e) =>
            {
                var item = e.Item as TreeRepositoryHeaderVM;
                if (item == null) 
                { 
                    e.Accepted = false; 
                    return; 
                }
                e.Accepted = item.IsFavorite;
            };

            LastTreeRepositoryHeadersVMs = new CollectionViewSource { Source = TreeRepositoryHeadersVMs };
            LastTreeRepositoryHeadersVMs.Filter += (s, e) =>
            {
                var item = e.Item as TreeRepositoryHeaderVM;
                if (item == null)
                {
                    e.Accepted = false;
                    return;
                }
                e.Accepted = DateTime.UtcNow - item.LastOpening <= TimeSpan.FromDays(90);
            };

            TreeRepositoryHeadersVMs.CollectionChanged += (s, e) =>
            {
                FavoriteTreeRepositoryHeadersVMs.View.Refresh();
                LastTreeRepositoryHeadersVMs.View.Refresh();
            };
        }
        public bool CheckTreeRepositoryAvailable(TreeRepositoryHeaderVM header)
        {
            if (header == null)
                return false;
            if (CheckTreeRepositoryAvailableAction == null)
                return false;
            CheckTreeRepositoryAvailableAction.Invoke(header);
            return header.IsTreeRepositoryAvailable;
        }
        private ObservableCollection<TreeRepositoryHeaderVM> LoadTreeRepositoryHeadersVMs() 
        {
            _treeRepositoryHeadersVMs.Clear();
            var headers = _service.GetTreeRepositoryHeadersCollection();
            if (headers == null)
                return null;
            headers.OrderByDescending(x => x.LastOpening);

            foreach (var header in headers)
            {
                var vm = new TreeRepositoryHeaderVM(header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updateTreeRepositoryHeaders, _configurationService, _appConfig, _treeRepositoryHeadersCollectionConfig);
                CheckTreeRepositoryAvailable(vm);
                _treeRepositoryHeadersVMs.Add(vm);
            }
            return TreeRepositoryHeadersVMs;
        }
        internal TreeRepositoryHeaderVM  AddTreeRepositoryHeaderVMFromTreeRepositoryVM(TreeRepositoryVM treeRepositoryVM)
        {
            var header = _service.CreateTreeRepositoryHeaderFromTreeRepository(treeRepositoryVM.Model);

            _treeRepositoryHeadersCollectionConfig.Value.TreeRepositoryHeaders.Add(header.ToDbEntity());
            _configurationService.UpdateConfigFile(_appConfig.Value.RepositoryHeadersConfigFullPath, _treeRepositoryHeadersCollectionConfig);

            var result = new TreeRepositoryHeaderVM(header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updateTreeRepositoryHeaders, _configurationService, _appConfig, _treeRepositoryHeadersCollectionConfig);
            TreeRepositoryHeadersVMs.Add(result);
            return result;
        }
    }
}
