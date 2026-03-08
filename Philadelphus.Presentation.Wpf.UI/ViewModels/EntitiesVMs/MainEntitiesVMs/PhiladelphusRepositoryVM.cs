using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs
{
    public class PhiladelphusRepositoryVM : MainEntityBaseVM<PhiladelphusRepositoryModel>
    {
        public IDataStorageModel OwnDataStorage 
        {
            get
            {
                return _model.OwnDataStorage;
            }
        }

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

        public string ChildsCount { get => $"Детей: {Childs?.Count()}, Корней: {_model?.ContentShrub?.ContentTrees?.Count()}, Uuids: NOT IMPLEMENTED"; }

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


        public PhiladelphusRepositoryVM(
            PhiladelphusRepositoryModel repositoryModel,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service)
            : base(repositoryModel, dataStoragesCollectionVM, service)
        {
            foreach (var item in repositoryModel.ContentShrub.ContentTrees.Select(x => x.ContentRoot))
            {
                Childs.Add(new TreeRootVM(item, _dataStoragesCollectionVM, service));
            }
        }
    }
}
