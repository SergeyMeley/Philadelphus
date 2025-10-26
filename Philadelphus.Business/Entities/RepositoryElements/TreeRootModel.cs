using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.RepositoryElements
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
        internal TreeRootModel(Guid guid, IParentModel parent, IDataStorageModel dataStorage, IMainEntity dbEntity) : base(guid, parent, dbEntity)
        {
            Guid = guid;
            Parent = parent;
            ParentRepository = (TreeRepositoryModel)parent;
            OwnDataStorage = dataStorage;
            Initialize();
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
