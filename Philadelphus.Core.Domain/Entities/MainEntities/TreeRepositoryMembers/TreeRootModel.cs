using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
{
    public class TreeRootModel : TreeRepositoryMemberBaseModel, IHavingOwnDataStorageModel, IChildrenModel, IParentModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый корень"; }
        public override EntityTypesModel EntityType { get => EntityTypesModel.Root; }
        
        private IDataStorageModel _ownDataStorage;
        public IDataStorageModel OwnDataStorage
        {
            get
            {
                return _ownDataStorage;
            }
            private set
            {
                _ownDataStorage = value;
                DataStorages.Add(value);
            }
        }
        public override IDataStorageModel DataStorage { get => OwnDataStorage; }
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();
        public EntityElementTypeModel ElementType { get; set; }
        public IParentModel Parent {  get; private set; }
        public List<TreeNodeModel> ChildTreeNodes { get => Childs.Where(x => x.GetType() == typeof(TreeNodeModel)).Cast<TreeNodeModel>().ToList(); }
        public List<IChildrenModel> Childs { get; set; }
        internal TreeRootModel(Guid uuid, TreeRepositoryModel parent, IDataStorageModel dataStorage, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
        {
            if (SetParents(parent))
            {
                OwnDataStorage = dataStorage;
                Initialize();
            }
        }
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            //foreach (var item in ParentRepository.ElementsCollection)
            //{
            //    existNames.Add(item.Name);
            //}
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            OwnDataStorage = storage;
            return true;
        }
    }
}
