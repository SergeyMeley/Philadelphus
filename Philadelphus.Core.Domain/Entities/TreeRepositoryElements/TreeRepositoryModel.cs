using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Helpers.InfrastructureConverters;
using Philadelphus.Business.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRepositoryModel : ITreeRepositoryHeaderModel, IHavingOwnDataStorageModel, IParentModel
    {
        protected virtual string DefaultFixedPartOfName { get => "Новый репозиторий"; }

        private Guid _guid;
        public Guid Guid 
        { 
            get
            {
                return _guid;
            }
            protected set
            {
                if (_guid != value)
                {
                    _guid = value;
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }
        public AuditInfoModel AuditInfo { get; set; } = new AuditInfoModel();
        public State State { get; internal set; } = State.Initialized;
        public TreeRepository DbEntity { get; set; }

        private IDataStorageModel _ownDataStorage;
        public IDataStorageModel OwnDataStorage
        {
            get
            {
                return _ownDataStorage;
            }
            private set
            {
                if (_ownDataStorage != value)
                {
                    _ownDataStorage = value;
                    DataStorages.Add(value);
                    if (State != State.Initialized && State != State.SoftDeleted)
                    {
                        State = State.Changed;
                    }
                }
            }
        }

        public string OwnDataStorageName { get => _ownDataStorage.Name; set => OwnDataStorageName = value; }    //TODO: Исправить костыль
        public Guid OwnDataStorageUuid { get => _ownDataStorage.Guid; set => OwnDataStorageUuid = value; }      //TODO: Исправить костыль
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();
        //TODO
        public List<TreeRootModel> ChildTreeRoots { get => Childs.Where(x => x.GetType() == typeof(TreeRootModel)).Cast<TreeRootModel>().ToList(); }
        //TODO
        public List<Guid> ChildsGuids {  get; internal set; }
        public List<IChildrenModel> Childs { get; internal set; }
        public List<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
        internal TreeRepositoryModel(Guid guid, IDataStorageModel dataStorage, TreeRepository dbEntity)
        {
            Guid = guid;
            OwnDataStorage = dataStorage;
            DbEntity = dbEntity;
            Initialize();
        }

        internal TreeRepositoryModel(TreeRepositoryHeaderModel headerModel)
        {
            Guid = headerModel.Guid;
            Initialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendLine();
            sb.Append(Guid);
            return sb.ToString();
        }

        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new string[0], DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
        }

        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            throw new NotImplementedException();
        }

    }
}
