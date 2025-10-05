using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRepositoryModel : MainEntityBaseModel, IHavingOwnDataStorageModel, IParentModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый репозиторий"; }
        public override EntityTypesModel EntityType { get => EntityTypesModel.Repository; }
        public IDataStorageModel OwnDataStorage { get; private set; }
        public IEnumerable<IChildrenModel> Childs { get; internal set; }
        public IEnumerable<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();
        internal TreeRepositoryModel(Guid guid, IDataStorageModel dataStorage) : base(guid)
        {
            Guid = guid;
            OwnDataStorage = dataStorage;
            Initialize();
        }
        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new string[0], DefaultFixedPartOfName);
            Childs = new ObservableCollection<IChildrenModel>();
        }

        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            throw new NotImplementedException();
        }

    }
}
