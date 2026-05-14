using AutoMapper;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    /// <summary>
    /// Модель представления для заголовка репозитория Чубушника.
    /// </summary>
    public class PhiladelphusRepositoryHeaderVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly IMapper _mapper;
        private readonly IPhiladelphusRepositoryCollectionService _service;
        private readonly DataStorageVM _dataStoragesVM;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<PhiladelphusRepositoryHeadersCollectionConfig> _PhiladelphusRepositoryHeadersCollectionConfig;

        private readonly PhiladelphusRepositoryHeaderModel _model;

        private readonly Action _updatePhiladelphusRepositoryHeaders;

        public Guid Uuid
        {
            get
            {
                return _model.Uuid;
            }
        }
        public string Name
        {
            get
            {
                return _model.Name;
            }
            set
            {
                _model.Name = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(Name));
            }
        }
        public string? Description
        {
            get
            {
                return _model.Description;
            }
            set
            {
                _model.Description = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(Description));
            }
        }
        public string OwnDataStorageName
        {
            get
            {
                return _model.OwnDataStorageName;
            }
            set
            {
                _model.OwnDataStorageName = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(OwnDataStorageName));
            }
        }
        public Guid OwnDataStorageUuid
        {
            get
            {
                return _model.OwnDataStorageUuid;
            }
            set
            {
                _model.OwnDataStorageUuid = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(OwnDataStorageUuid));
            }
        }
        public DateTime? LastOpening
        {
            get
            {
                return _model.LastOpening;
            }
            set
            {
                _model.LastOpening = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(LastOpening));
            }
        }
        public bool IsFavorite
        {
            get
            {
                return _model.IsFavorite;
            }
            set
            {
                _model.IsFavorite = value;
                SaveRepositoryHeader();
                _updatePhiladelphusRepositoryHeaders.Invoke();
                OnPropertyChanged(nameof(IsFavorite));
            }
        }
        public bool IsHidden
        {
            get
            {
                return _model.IsHidden;
            }
            set
            {
                _model.IsHidden = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(IsHidden));
            }
        }
        public State State
        {
            get
            {
                return _model.State;
            }
        }
        private bool _isPhiladelphusRepositoryAvailable;
        public bool IsPhiladelphusRepositoryAvailable 
        { 
            get
            {
                return _isPhiladelphusRepositoryAvailable;
            }
            set
            {
                _isPhiladelphusRepositoryAvailable = value;
                OnPropertyChanged(nameof(IsPhiladelphusRepositoryAvailable));
            }
        }

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PhiladelphusRepositoryHeaderVM" />.
        /// </summary>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="philadelphusRepositoryHeader">Параметр philadelphusRepositoryHeader.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="dataStoragesVM">Параметр dataStoragesVM.</param>
        /// <param name="updatePhiladelphusRepositoryHeaders">Параметр updatePhiladelphusRepositoryHeaders.</param>
        /// <param name="configurationService">Параметр configurationService.</param>
        /// <param name="appConfig">Параметр appConfig.</param>
        /// <param name="PhiladelphusRepositoryHeadersCollectionConfig">Параметр PhiladelphusRepositoryHeadersCollectionConfig.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public PhiladelphusRepositoryHeaderVM(
            IMapper mapper,
            PhiladelphusRepositoryHeaderModel philadelphusRepositoryHeader,
            IPhiladelphusRepositoryCollectionService service,
            DataStorageVM dataStoragesVM, 
            Action updatePhiladelphusRepositoryHeaders,
            IConfigurationService configurationService,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<PhiladelphusRepositoryHeadersCollectionConfig> PhiladelphusRepositoryHeadersCollectionConfig)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            ArgumentNullException.ThrowIfNull(philadelphusRepositoryHeader);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(dataStoragesVM);
            ArgumentNullException.ThrowIfNull(updatePhiladelphusRepositoryHeaders);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(appConfig.Value);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryHeadersCollectionConfig);
            ArgumentNullException.ThrowIfNull(PhiladelphusRepositoryHeadersCollectionConfig.Value);

            _mapper = mapper;
            _model = philadelphusRepositoryHeader;
            _service = service;
            _dataStoragesVM = dataStoragesVM;
            _configurationService = configurationService;
            _appConfig = appConfig;
            _PhiladelphusRepositoryHeadersCollectionConfig = PhiladelphusRepositoryHeadersCollectionConfig;

            _updatePhiladelphusRepositoryHeaders = updatePhiladelphusRepositoryHeaders;
        }

        #endregion

        #region [Commands]



        #endregion

        #region [ Methods ]

        private bool SaveRepositoryHeader()
        {

            var headers = _PhiladelphusRepositoryHeadersCollectionConfig.Value.PhiladelphusRepositoryHeaders;

            if (headers.Any(x => x.Uuid == _model.Uuid) == false)
            {
                headers.Add(_mapper.Map<PhiladelphusRepositoryHeader>(_model));
            }
            else
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    if (headers[i].Uuid == _model.Uuid)
                    {
                        headers[i] = _mapper.Map<PhiladelphusRepositoryHeader>(_model);
                        break;
                    }
                }
            }

            _configurationService.UpdateConfigFile(_appConfig.Value.RepositoryHeadersConfigFullPath, _PhiladelphusRepositoryHeadersCollectionConfig);

            return true;
        }

        #endregion
    }
}
