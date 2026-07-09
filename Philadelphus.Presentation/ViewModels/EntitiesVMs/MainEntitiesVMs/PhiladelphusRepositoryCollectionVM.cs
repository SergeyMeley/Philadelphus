using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.IO;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs
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
        private readonly IFileDialogService? _fileDialogService;
        private readonly IRelayCommandFactory _commandFactory;

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
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryCollectionVM(
            IServiceProvider serviceProvider,
            ILogger logger,
            INotificationService notificationService,
            IPhiladelphusRepositoryCollectionService collectionService,
            IPhiladelphusRepositoryService service,
            DataStoragesCollectionVM dataStoragesSettings,
            IOptions<ApplicationSettingsConfig> options,
            IFileDialogService fileDialogService,
            IRelayCommandFactory commandFactory)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(collectionService);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);
            ArgumentNullException.ThrowIfNull(dataStoragesSettings);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _serviceProvider = serviceProvider;
            _logger = logger;
            _notificationService = notificationService;
            _collectionService = collectionService;
            _service = service;
            _fileDialogService = fileDialogService;
            _commandFactory = commandFactory;

            _dataStoragesSettingsVM = dataStoragesSettings;

            InitRepositoriesVMsCollection();
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

        public ObservableCollection<PhiladelphusRepositoryVM> VisiblePhiladelphusRepositoriesVMs { get; }
            = new ObservableCollection<PhiladelphusRepositoryVM>();

        private bool _showHiddenPhiladelphusRepositories;
        public bool ShowHiddenPhiladelphusRepositories
        {
            get
            {
                return _showHiddenPhiladelphusRepositories;
            }
            set
            {
                if (_showHiddenPhiladelphusRepositories == value)
                    return;

                _showHiddenPhiladelphusRepositories = value;
                RefreshVisiblePhiladelphusRepositoriesVMs();
                OnPropertyChanged(nameof(ShowHiddenPhiladelphusRepositories));
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

        public IRelayCommand AddExistRepository
        {
            get
            {
                return _commandFactory.Create(obj =>
                {
                    _collectionService.AddExistPhiladelphusRepository(new DirectoryInfo(""));
                });
            }
        }

        public IRelayCommand CreateNewRepository
        {
            get
            {
                return _commandFactory.Create(obj =>
                {
                    var builder = new DataStorageBuilder();
                    var repository = _collectionService.CreateNewPhiladelphusRepository(builder.Build());
                    var repositorVM = new PhiladelphusRepositoryVM(repository, _dataStoragesSettingsVM, _service, _fileDialogService, _notificationService);
                    PhiladelphusRepositoriesVMs.Add(repositorVM);
                     
                });
            }
        }
        private bool InitRepositoriesVMsCollection()
        {
            PhiladelphusRepositoriesVMs.CollectionChanged += PhiladelphusRepositoriesVMsCollectionChanged;

            var storages = _dataStoragesSettingsVM.DataStoragesVMs.Select(x => x.Model);
            var repositories = _collectionService.GetPhiladelphusRepositoriesCollection(storages);
            if (repositories == null)
                return false;
            foreach (var item in repositories)
            {
                _PhiladelphusRepositoriesVMs.Add(new PhiladelphusRepositoryVM(item, _dataStoragesSettingsVM, _service, _fileDialogService, _notificationService));
            }
            RefreshVisiblePhiladelphusRepositoriesVMs();
            return true;
        }

        private void PhiladelphusRepositoriesVMsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PhiladelphusRepositoryVM repository in e.OldItems)
                {
                    repository.PropertyChanged -= PhiladelphusRepositoryVMPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (PhiladelphusRepositoryVM repository in e.NewItems)
                {
                    repository.PropertyChanged += PhiladelphusRepositoryVMPropertyChanged;
                }
            }

            RefreshVisiblePhiladelphusRepositoriesVMs();
        }

        private void PhiladelphusRepositoryVMPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PhiladelphusRepositoryVM.IsHidden))
            {
                if (sender is PhiladelphusRepositoryVM repository)
                {
                    var model = repository.Model;
                    _collectionService.SaveChanges(ref model);
                }

                RefreshVisiblePhiladelphusRepositoriesVMs();
            }
        }

        private void RefreshVisiblePhiladelphusRepositoriesVMs()
        {
            VisiblePhiladelphusRepositoriesVMs.Clear();
            foreach (var repository in PhiladelphusRepositoriesVMs.Where(x => ShowHiddenPhiladelphusRepositories || x.IsHidden == false))
            {
                VisiblePhiladelphusRepositoriesVMs.Add(repository);
            }
            OnPropertyChanged(nameof(VisiblePhiladelphusRepositoriesVMs));
        }
        public bool CheckPhiladelphusRepositoryVMAvailable(Guid uuid, out PhiladelphusRepositoryVM outPhiladelphusRepositoryVM)
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
            var result = new PhiladelphusRepositoryVM(repository, _dataStoragesSettingsVM, _service, _fileDialogService, _notificationService);
            _PhiladelphusRepositoriesVMs.Add(result);
            return result;
        }
    }
}
