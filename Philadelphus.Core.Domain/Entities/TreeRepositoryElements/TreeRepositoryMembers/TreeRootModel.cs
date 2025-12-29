using Philadelphus.Core.Domain.Entities.ElementsProperties;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers
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
