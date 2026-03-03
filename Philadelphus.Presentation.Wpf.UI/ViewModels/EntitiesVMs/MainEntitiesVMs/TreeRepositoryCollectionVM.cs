using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;
using System.IO;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class PhiladelphusRepositoryCollectionVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PhiladelphusRepositoryCollectionVM> _logger;
        private readonly INotificationService _notificationService;
        private readonly IPhiladelphusRepositoryCollectionService _collectionService;
        private readonly IPhiladelphusRepositoryService _service;

        private DataStoragesCollectionVM _dataStoragesSettingsVM;
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
        public PhiladelphusRepositoryCollectionVM(
            IServiceProvider serviceProvider,
            ILogger<PhiladelphusRepositoryCollectionVM> logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService collectionService, 
            IPhiladelphusRepositoryService service,
            DataStoragesCollectionVM dataStoragesSettings,
            IOptions<ApplicationSettingsConfig> options)
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
        private static PhiladelphusRepositoryVM _currentRepositoryVM;
        public PhiladelphusRepositoryVM CurrentRepositoryVM
        {
            get
            {
                return _currentRepositoryVM;
            }
            set
            {
                _currentRepositoryVM = null;
                _currentRepositoryVM = value;
                //_currentRepositoryVM.LoadPhiladelphusRepository();    //TODO: Перенести в другое место
                OnPropertyChanged(nameof(CurrentRepositoryVM));
                OnPropertyChanged(nameof(PropertyList));
                OnPropertyChanged(nameof(PhiladelphusRepositoriesVMs));
            }
        }

        private ObservableCollection<PhiladelphusRepositoryVM> _PhiladelphusRepositoriesVMs = new ObservableCollection<PhiladelphusRepositoryVM>();
        public ObservableCollection<PhiladelphusRepositoryVM> PhiladelphusRepositoriesVMs
        {
            get => _PhiladelphusRepositoriesVMs;
            //private set
            //{
            //    _PhiladelphusRepositoriesVMs = value.ToList();
            //    OnPropertyChanged(nameof(PhiladelphusRepositoriesVMs));
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
                //return PropertyGridHelper.GetProperties(_currentRepositoryExplorerVM.PhiladelphusRepository);
                return null;
            }
        }

        public RelayCommand AddExistRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    _collectionService.AddExistPhiladelphusRepository(new DirectoryInfo(""));
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
                    var repository = _collectionService.CreateNewPhiladelphusRepository(builder.Build());
                    var repositorVM = new PhiladelphusRepositoryVM(repository, _service);
                    PhiladelphusRepositoriesVMs.Add(repositorVM);
                     
                });
            }
        }
        private bool InitRepositoriesVMsCollection()
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model);
            var repositories = _collectionService.GetPhiladelphusRepositoriesCollection(storages);
            if (repositories == null)
                return false;
            foreach (var item in repositories)
            {
                _PhiladelphusRepositoriesVMs.Add(new PhiladelphusRepositoryVM(item, _service));
            }
            return true;
        }
        internal bool CheckPhiladelphusRepositoryVMAvailable(Guid uuid, out PhiladelphusRepositoryVM outPhiladelphusRepositoryVM)
        {
            outPhiladelphusRepositoryVM = PhiladelphusRepositoriesVMs.FirstOrDefault(x => x.Uuid == uuid);
            if (outPhiladelphusRepositoryVM != null && outPhiladelphusRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            outPhiladelphusRepositoryVM = InitPhiladelphusRepositoryVM(uuid);
            if (outPhiladelphusRepositoryVM != null && outPhiladelphusRepositoryVM.OwnDataStorage.IsAvailable == true)
                return true;
            return false;
        }
        private PhiladelphusRepositoryVM InitPhiladelphusRepositoryVM(Guid uuid)
        {
            var storages = _dataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model);
            var repositories = _collectionService.GetPhiladelphusRepositoriesCollection(storages, new[] { uuid });
            if (repositories == null)
                return null;
            var repository = repositories.FirstOrDefault(x => x.Uuid == uuid);
            if (repository == null)
                return null;
            var result = new PhiladelphusRepositoryVM(repository, _service);
            _PhiladelphusRepositoriesVMs.Add(result);
            return result;
        }
    }
}
