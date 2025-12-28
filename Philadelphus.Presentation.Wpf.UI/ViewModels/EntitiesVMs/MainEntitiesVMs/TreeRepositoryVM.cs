using Philadelphus.Core.Domain.Entities.ElementsProperties;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class TreeRepositoryVM : ViewModelBase   
    {
        private readonly TreeRepositoryModel _model;
        public TreeRepositoryModel Model
        {
            get
            {
                return _model;
            }
        }

        public Guid Guid { get => _model.Guid; }
        public string Name { get => _model.Name; set => _model.Name = value; }
        public string Description { get => _model.Description; set => _model.Description = value; }
        public AuditInfoModel AuditInfo { get => _model.AuditInfo; }
        public State State { get => _model.State; }
        public IDataStorageModel OwnDataStorage { get => _model.OwnDataStorage; }

        private DataStorageVM _storageVM;
        public DataStorageVM StorageVM { get => _storageVM; }
        public string OwnDataStorageName
        {
            get
            {
                return _model.OwnDataStorageName;
            }
        }
        public Guid OwnDataStorageUuid
        {
            get
            {
                return _model.OwnDataStorageUuid;
            }
        }

        private ObservableCollection<IDataStorageModel> _dataStorages { get; }
        public ObservableCollection<IDataStorageModel> DataStorages { get => _dataStorages; }

        private ObservableCollection<TreeRootVM> _childs = new ObservableCollection<TreeRootVM>();
        public ObservableCollection<TreeRootVM> Childs { get => _childs; }

        public string ChildsCount { get => $"Детей: {Childs.Count()}, Корней: {_model.ChildTreeRoots?.Count()}, Uuids: {_model.ChildsGuids?.Count}"; }

        public bool IsFavorite
        {
            get
            {
                return _model.IsFavorite;
            }
            set
            {
                _model.IsFavorite = value;
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
            }
        }


        public TreeRepositoryVM(
            TreeRepositoryModel treeRepositoryModel,
            ITreeRepositoryService service)
        {
            _model = treeRepositoryModel;
            _storageVM = new DataStorageVM(treeRepositoryModel.OwnDataStorage);
            foreach (var item in treeRepositoryModel.Childs)
            {
                if (item.GetType() == typeof(TreeRootModel))
                {
                    Childs.Add(new TreeRootVM((TreeRootModel)item, service));
                }
            }
        }

    }
}
