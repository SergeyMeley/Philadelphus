using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class TreeRepositoryHeaderVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly ITreeRepositoryCollectionService _service;
        private readonly DataStorageVM _dataStoragesVM;
        private readonly IConfigurationService _configurationService;
        private readonly IOptions<ApplicationSettingsConfig> _appConfig;
        private readonly IOptions<TreeRepositoryHeadersCollectionConfig> _treeRepositoryHeadersCollectionConfig;

        private readonly TreeRepositoryHeaderModel _model;

        private readonly Action _updateTreeRepositoryHeaders;

        public Guid Uuid
        {
            get
            {
                return _model.Uuid;
            }
            set
            {
                _model.Uuid = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(Uuid));
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
                _updateTreeRepositoryHeaders.Invoke();
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
        private bool _isTreeRepositoryAvailable;
        public bool IsTreeRepositoryAvailable 
        { 
            get
            {
                return _isTreeRepositoryAvailable;
            }
            set
            {
                _isTreeRepositoryAvailable = value;
                OnPropertyChanged(nameof(IsTreeRepositoryAvailable));
            }
        }

        #endregion

        #region [ Construct ]

        public TreeRepositoryHeaderVM(
            TreeRepositoryHeaderModel treeRepositoryHeader,
            ITreeRepositoryCollectionService service,
            DataStorageVM dataStoragesVM, 
            Action updateTreeRepositoryHeaders,
            IConfigurationService configurationService,
            IOptions<ApplicationSettingsConfig> appConfig,
            IOptions<TreeRepositoryHeadersCollectionConfig> treeRepositoryHeadersCollectionConfig)
        {
            _model = treeRepositoryHeader;
            _service = service;
            _dataStoragesVM = dataStoragesVM;
            _configurationService = configurationService;
            _appConfig = appConfig;
            _treeRepositoryHeadersCollectionConfig = treeRepositoryHeadersCollectionConfig;

            _updateTreeRepositoryHeaders = updateTreeRepositoryHeaders;
        }

        #endregion

        #region [Commands]



        #endregion

        #region [ Methods ]

        private bool SaveRepositoryHeader()
        {

            var headers = _treeRepositoryHeadersCollectionConfig.Value.TreeRepositoryHeaders;

            if (headers.Any(x => x.Uuid == _model.Uuid) == false)
            {
                headers.Add(_model.ToDbEntity());
            }
            else
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    if (headers[i].Uuid == _model.Uuid)
                    {
                        headers[i] = _model.ToDbEntity();
                        break;
                    }
                }
            }

            _configurationService.UpdateConfigFile(_appConfig.Value.RepositoryHeadersConfigFullPath, _treeRepositoryHeadersCollectionConfig);

            return true;
        }

        #endregion
    }
}
