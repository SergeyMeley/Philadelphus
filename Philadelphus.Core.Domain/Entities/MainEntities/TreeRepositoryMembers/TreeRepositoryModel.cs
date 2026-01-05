using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Text;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
{
    public class TreeRepositoryModel : ITreeRepositoryHeaderModel, IHavingOwnDataStorageModel, IParentModel
    {
        protected virtual string DefaultFixedPartOfName { get => "Новый репозиторий"; }

        private Guid _uuid;
        public Guid Uuid 
        { 
            get
            {
                return _uuid;
            }
            protected set
            {
                if (_uuid != value)
                {
                    _uuid = value;
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
        public Guid OwnDataStorageUuid { get => _ownDataStorage.Uuid; set => OwnDataStorageUuid = value; }      //TODO: Исправить костыль
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();
        //TODO
        public List<TreeRootModel> ChildTreeRoots { get => Childs.Where(x => x.GetType() == typeof(TreeRootModel)).Cast<TreeRootModel>().ToList(); }
        //TODO
        public List<Guid> ChildsUuids {  get; internal set; }
        public List<IChildrenModel> Childs { get; internal set; }
        public List<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();
        public DateTime? LastOpening { get; set; }
        public bool IsFavorite { get; set; }
        internal TreeRepositoryModel(Guid uuid, IDataStorageModel dataStorage, TreeRepository dbEntity)
        {
            Uuid = uuid;
            OwnDataStorage = dataStorage;
            DbEntity = dbEntity;
            Initialize();
        }

        internal TreeRepositoryModel(TreeRepositoryHeaderModel headerModel)
        {
            Uuid = headerModel.Uuid;
            Initialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.AppendLine();
            sb.Append(Uuid);
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
