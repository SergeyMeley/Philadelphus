using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class TreeRepositoryHeaderVM : ViewModelBase //TODO: Вынести команды в RepositoryExplorerControlVM, исключить сервисы
    {
        #region [ Props ]

        private readonly ITreeRepositoryCollectionService _service;
        private readonly DataStorageVM _dataStoragesVM;

        private readonly TreeRepositoryHeaderModel _model;

        private readonly Action _updateTreeRepositoryHeaders;

        public Guid Guid
        {
            get
            {
                return _model.Guid;
            }
            set
            {
                _model.Guid = value;
                SaveRepositoryHeader();
                OnPropertyChanged(nameof(Guid));
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
            Action updateTreeRepositoryHeaders)
        {
            _model = treeRepositoryHeader;
            _service = service;
            _dataStoragesVM = dataStoragesVM;

            _updateTreeRepositoryHeaders = updateTreeRepositoryHeaders;
        }

        #endregion

        #region [Commands]



        #endregion

        #region [ Methods ]

        private bool SaveRepositoryHeader()
        {
            _service.SaveChanges(_model, _dataStoragesVM.Model);
            return true;
        }

        #endregion
    }
}
