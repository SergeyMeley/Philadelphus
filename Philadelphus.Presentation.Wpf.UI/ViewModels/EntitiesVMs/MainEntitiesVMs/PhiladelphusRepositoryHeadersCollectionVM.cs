using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Модель представления для коллекции заголовков репозиториев Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryHeadersCollectionVM : ViewModelBase  //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IPhiladelphusRepositoryCollectionService _service;
        private readonly DataStoragesCollectionVM _dataStoragesSettingsVM;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _philadelphusRepositoryHeadersCollectionConfig;

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

        /// <summary>
        /// Заголовок репозитория Чубушника.
        /// </summary>
        public CollectionViewSource FavoritePhiladelphusRepositoryHeadersVMs { get; }
       
        /// <summary>
        /// Заголовок репозитория Чубушника.
        /// </summary>
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
      
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepositoryHeadersCollectionVM" />.
        /// </summary>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="dataStoragesSettingsVM">Параметр dataStoragesSettingsVM.</param>
        /// <param name="configurationService">Параметр configurationService.</param>
        /// <param name="appConfig">Параметр appConfig.</param>
        /// <param name="philadelphusRepositoryHeadersCollectionConfig">Параметр philadelphusRepositoryHeadersCollectionConfig.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryHeadersCollectionVM(
            ILogger logger,
            INotificationService notificationService,
            IMapper mapper,
            IPhiladelphusRepositoryCollectionService service,
            DataStoragesCollectionVM dataStoragesSettingsVM,
            IConfigurationService configurationService,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> philadelphusRepositoryHeadersCollectionConfig)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(notificationService);
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(dataStoragesSettingsVM);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(appConfig.Value);
            ArgumentNullException.ThrowIfNull(philadelphusRepositoryHeadersCollectionConfig);
            ArgumentNullException.ThrowIfNull(philadelphusRepositoryHeadersCollectionConfig.Value);

            _logger = logger;
            _notificationService = notificationService;
            _mapper = mapper;
            _service = service;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _configurationService = configurationService;
            _appConfig = appConfig;
            _philadelphusRepositoryHeadersCollectionConfig = philadelphusRepositoryHeadersCollectionConfig;

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
    
        /// <summary>
        /// Выполняет операцию репозитория Чубушника.
        /// </summary>
        /// <param name="header">Параметр header.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
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
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(config.Value);

            _PhiladelphusRepositoryHeadersVMs.Clear();
            var headers = config.Value.PhiladelphusRepositoryHeaders;
            if (headers == null)
                return null;
            headers.OrderByDescending(x => x.LastOpening);

            foreach (var header in headers)
            {
                var vm = new PhiladelphusRepositoryHeaderVM(_mapper, _mapper.Map<PhiladelphusRepositoryHeaderModel>(header), _service, _dataStoragesSettingsVM.MainDataStorageVM, _updatePhiladelphusRepositoryHeaders, _configurationService, _appConfig, _philadelphusRepositoryHeadersCollectionConfig);
                CheckPhiladelphusRepositoryAvailable(vm);
                _PhiladelphusRepositoryHeadersVMs.Add(vm);
            }
            return PhiladelphusRepositoryHeadersVMs;
        }
        internal PhiladelphusRepositoryHeaderVM  AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(PhiladelphusRepositoryVM PhiladelphusRepositoryVM)
        {
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryVM);

            var header = _service.CreatePhiladelphusRepositoryHeaderFromPhiladelphusRepository(PhiladelphusRepositoryVM.Model);

            _philadelphusRepositoryHeadersCollectionConfig.Value.PhiladelphusRepositoryHeaders.Add(_mapper.Map<PhiladelphusRepositoryHeader>(header));
            _configurationService.UpdateConfigFile(_appConfig.Value.RepositoryHeadersConfigFullPath, _philadelphusRepositoryHeadersCollectionConfig);

            var result = new PhiladelphusRepositoryHeaderVM(_mapper, header, _service, _dataStoragesSettingsVM.MainDataStorageVM, _updatePhiladelphusRepositoryHeaders, _configurationService, _appConfig, _philadelphusRepositoryHeadersCollectionConfig);
            PhiladelphusRepositoryHeadersVMs.Add(result);
            return result;
        }
    }
}
