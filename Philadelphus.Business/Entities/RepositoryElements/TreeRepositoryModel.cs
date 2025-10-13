using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
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
    public class TreeRepositoryModel : IHavingOwnDataStorageModel, IParentModel
    {
        protected virtual string DefaultFixedPartOfName { get => "Новый репозиторий"; }
        public Guid Guid { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AuditInfoModel AuditInfo { get; private set; } = new AuditInfoModel();
        public State State { get; set; } = State.Initialized;
        public IMainEntity DbEntity { get; set; }

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
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();
        public List<TreeRootModel> ChildTreeRoots { get => Childs.Where(x => x.GetType() == typeof(TreeRootModel)).Cast<TreeRootModel>().ToList(); }
        public List<Guid> ChildsGuids { get; set; }
        public List<IChildrenModel> Childs { get; internal set; }
        public List<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();
        public TreeRepositoryModel(Guid guid, IDataStorageModel dataStorage)
        {
            Guid = guid;
            OwnDataStorage = dataStorage;
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
