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
    public class TreeRepositoryModel : MainEntityBaseModel, IHavingOwnStorageModel, IParentModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Repository; }
        public InfrastructureTypes DefaultInfrastructureRepositoryType { get; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public IEnumerable<IMainEntitiesInfrastructure> InfrastructureRepositories { get; set; }
        //public IEnumerable<IDataStorage> DataStorages { get; set; }
        public IEnumerable<IChildrenModel> Childs { get; private set; }
        public IEnumerable<TreeRepositoryMemberBaseModel> ElementsCollection { get; internal set; } = new List<TreeRepositoryMemberBaseModel>();
        internal TreeRepositoryModel(Guid guid) : base(guid)
        {
            Guid = guid;
            Initialize();
        }
        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new string[0], "Новый репозиторий");
            Childs = new ObservableCollection<IChildrenModel>();
        }

        public bool SetInfrastructureRepository(IMainEntitiesInfrastructure infrastructure)
        {
            Infrastructure = infrastructure;
            return true;
        }

    }
}
