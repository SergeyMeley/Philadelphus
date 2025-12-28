  using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;

namespace Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class TreeRepositoryHeadersCollectionVM : ViewModelBase  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly ILogger<TreeRepositoryHeadersCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryCollectionService _service;
        private readonly DataStoragesSettingsVM _dataStoragesSettingsVM;

        private List<TreeRepositoryHeaderVM> _treeRepositoryHeadersVMs;
        public List<TreeRepositoryHeaderVM> TreeRepositoryHeadersVMs
        {
            get
            {
                if (_treeRepositoryHeadersVMs == null)
                {
                    _treeRepositoryHeadersVMs = new List<TreeRepositoryHeaderVM>();
                    LoadTreeRepositoryHeadersVMs();
                }
                return _treeRepositoryHeadersVMs;
            }
        }
        public List<TreeRepositoryHeaderVM> FavoriteTreeRepositoryHeadersVMs
        {
            get
            {
                return TreeRepositoryHeadersVMs.Where(x => x.IsFavorite).ToList();
            }
        }
        public List<TreeRepositoryHeaderVM> LastTreeRepositoryHeadersVMs
        {
            get
            {
                return TreeRepositoryHeadersVMs.OrderByDescending(x => x.LastOpening).Where(x => DateTime.UtcNow - x.LastOpening <= TimeSpan.FromDays(90)).ToList();
            }
        }

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
                });
            }
        }
        public TreeRepositoryHeadersCollectionVM(
            ILogger<TreeRepositoryHeadersCollectionVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService service,
            DataStoragesSettingsVM dataStoragesSettingsVM)
        {
            _logger = logger;
            _notificationService = notificationService;
            _service = service;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
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
        private List<TreeRepositoryHeaderVM> LoadTreeRepositoryHeadersVMs()
        {
            _treeRepositoryHeadersVMs.Clear();
            var headers = _service.ForceLoadTreeRepositoryHeadersCollection(_dataStoragesSettingsVM.MainDataStorageVM.Model);
            if (headers == null)
                return null;
            headers.OrderByDescending(x => x.LastOpening);

            foreach (var header in headers)
            {
                var vm = new TreeRepositoryHeaderVM(header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updateTreeRepositoryHeaders);
                CheckTreeRepositoryAvailable(vm);
                _treeRepositoryHeadersVMs.Add(vm);
            }
            return TreeRepositoryHeadersVMs;
        }
        internal TreeRepositoryHeaderVM AddTreeRepositoryHeaderVMFromTreeRepositoryVM(TreeRepositoryVM treeRepositoryVM)
        {
            var header = _service.CreateTreeRepositoryHeaderFromTreeRepository(treeRepositoryVM.Model);
            var result = new TreeRepositoryHeaderVM(header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updateTreeRepositoryHeaders);
            TreeRepositoryHeadersVMs.Add(result);
            return result;
        }
    }
}
