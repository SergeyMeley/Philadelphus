using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Модель представления для коллекции репозиториев Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryCollectionVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPhiladelphusRepositoryCollectionService _collectionService;
        private readonly IPhiladelphusRepositoryService _service;

        private DataStoragesCollectionVM _dataStoragesSettingsVM;

        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepositoryCollectionVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="collectionService">Параметр collectionService.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="dataStoragesSettings">Параметр dataStoragesSettings.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryCollectionVM(
            IServiceProvider serviceProvider,
            ILogger logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService collectionService, 
            IPhiladelphusRepositoryService service,
            DataStoragesCollectionVM dataStoragesSettings,
            IOptions<ApplicationSettingsConfig> options)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(collectionService);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(dataStoragesSettings);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);

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
                    var repositorVM = new PhiladelphusRepositoryVM(repository, _dataStoragesSettingsVM, _service);
                    PhiladelphusRepositoriesVMs.Add(repositorVM);
                     
                });
            }
        }
        private bool InitRepositoriesVMsCollection()
        {
            var storages = _dataStoragesSettingsVM.DataStoragesVMs.Select(x => x.Model);
            var repositories = _collectionService.GetPhiladelphusRepositoriesCollection(storages);
            if (repositories == null)
                return false;
            foreach (var item in repositories)
            {
                _PhiladelphusRepositoriesVMs.Add(new PhiladelphusRepositoryVM(item, _dataStoragesSettingsVM, _service));
            }
            return true;
        }
        internal bool CheckPhiladelphusRepositoryVMAvailable(Guid uuid, out PhiladelphusRepositoryVM outPhiladelphusRepositoryVM)
        {
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

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
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            var storages = _dataStoragesSettingsVM.DataStoragesVMs.Select(x => x.Model);
            var repositories = _collectionService.GetPhiladelphusRepositoriesCollection(storages, new[] { uuid });
            if (repositories == null)
                return null;
            var repository = repositories.FirstOrDefault(x => x.Uuid == uuid);
            if (repository == null)
                return null;
            var result = new PhiladelphusRepositoryVM(repository, _dataStoragesSettingsVM, _service);
            _PhiladelphusRepositoriesVMs.Add(result);
            return result;
        }
    }
}
