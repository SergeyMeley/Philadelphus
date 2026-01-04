using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class TreeRepositoryCollectionVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TreeRepositoryCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly ITreeRepositoryCollectionService _collectionService;
        private readonly ITreeRepositoryService _service;

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
        public TreeRepositoryCollectionVM(
            IServiceProvider serviceProvider,
            ILogger<TreeRepositoryCollectionVM> logger,
            INotificationService notificationService,
            ITreeRepositoryCollectionService collectionService, 
            ITreeRepositoryService service,
            DataStoragesSettingsVM dataStoragesSettings,
            IOptions<ApplicationSettings> options)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _notificationService = notificationService;
            _collectionService = collectionService;
            _service = service;

            _dataStoragesSettingsVM = dataStoragesSettings;

            InitRepositoriesVMsCollection();
            PropertyGridRepresentation = PropertyGridRepresentations.DataGrid;
        }
        private static TreeRepositoryVM _currentRepositoryVM;
        public TreeRepositoryVM CurrentRepositoryVM
        {
            get
            {
                return _currentRepositoryVM;
            }
            set
            {
                _currentRepositoryVM = null;
                _currentRepositoryVM = value;
                //_currentRepositoryVM.LoadTreeRepository();    //TODO: Перенести в другое место
                OnPropertyChanged(nameof(CurrentRepositoryVM));
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(TreeRepositoriesVMs));
            }
        }

        private ObservableCollection<TreeRepositoryVM> _treeRepositoriesVMs = new ObservableCollection<TreeRepositoryVM>();
        public ObservableCollection<TreeRepositoryVM> TreeRepositoriesVMs
        {
            get => _treeRepositoriesVMs;
            //private set
            //{
            //    _treeRepositoriesVMs = value.ToList();
            //    OnPropertyChanged(nameof(TreeRepositoriesVMs));
            //}
        }


        public List<string> PropertyGridRepresentationsCollection
        {
            get
            {
                var list = new List<string>();
                foreach (var item in Enum.GetNames(typeof(PropertyGridRepresentations)))
                {
                    list.Add(item);
                }
                return list;
            }
        }

        private PropertyGridRepresentations _propertyGridRepresentation;
        public PropertyGridRepresentations PropertyGridRepresentation
        {
            get
            {
                return _propertyGridRepresentation;
            }
            set
            {
                _propertyGridRepresentation = value;
                OnPropertyChanged(nameof(PropertyGridRepresentation));
            }
        }
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_currentRepositoryVM == null)
                    return null;
                //return PropertyGridHelper.GetProperties(_currentRepositoryExplorerVM.TreeRepository);
                return null;
            }
        }

        public RelayCommand AddExistRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _collectionService.AddExistTreeRepository(new DirectoryInfo(""));
                });
            }
        }

        public RelayCommand CreateNewRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var builder = new DataStorageBuilder();
                    var repository = _collectionService.CreateNewTreeRepository(builder.Build());
                    var repositorVM = new TreeRepositoryVM(repository, _service);
                    TreeRepositoriesVMs.Add(repositorVM);
                     
                });
            }
        }
        private bool InitRepositoriesVMsCollection()
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model);
            var repositories = _collectionService.GetTreeRepositoriesCollection(storages);
            if (repositories == null)
                return false;
            foreach (var item in repositories)
            {
                _treeRepositoriesVMs.Add(new TreeRepositoryVM(item, _service));
            }
            return true;
        }
        internal bool CheckTreeRepositoryVMAvailable(Guid uuid, out TreeRepositoryVM outTreeRepositoryVM)
        {
            outTreeRepositoryVM = TreeRepositoriesVMs.FirstOrDefault(x => x.Uuid == uuid);
            if (outTreeRepositoryVM != null && outTreeRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            outTreeRepositoryVM = InitTreeRepositoryVM(uuid);
            if (outTreeRepositoryVM != null && outTreeRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            return false;
        }
        private TreeRepositoryVM InitTreeRepositoryVM(Guid uuid)
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model);
            var repositories = _collectionService.GetTreeRepositoriesCollection(storages, new[] { uuid });
            if (repositories == null)
                return null;
            var repository = repositories.FirstOrDefault(x => x.Uuid == uuid);
            if (repository == null)
                return null;
            var result = new TreeRepositoryVM(repository, _service);
            _treeRepositoriesVMs.Add(result);
            return result;
        }
    }
}
