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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRepository : MainEntityBase, IHavingOwnStorage, IHavingChilds
    {
        public override EntityTypes EntityType { get => EntityTypes.Repository; }
        public InfrastructureTypes DefaultInfrastructureRepositoryType { get; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; } = new WindowsFileSystemRepository.Repositories.WindowsMainEntityRepository();
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public IEnumerable<IMainEntitiesInfrastructure> InfrastructureRepositories { get; set; }
        public IEnumerable<IDataStorage> DataStorages { get; set; }
        public IEnumerable<IHavingParent> Childs { get; private set; }
        public IEnumerable<RepositoryElementBase> ElementsCollection { get; internal set; } = new List<RepositoryElementBase>();
        public TreeRepository(Guid guid) : base(guid)
        {
            Guid = guid;
            Initialize();
        }
        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new string[0], "Новый репозиторий");
            Childs = new List<IHavingParent>();
        }
    }
}
