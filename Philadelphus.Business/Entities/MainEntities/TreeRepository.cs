using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRepository : MainEntityBase, IIndependentInfrastructure
    {
        public override EntityTypes EntityType { get => EntityTypes.Repository; }
        public InfrastructureRepositoryTypes DefaultInfrastructureRepositoryType { get; }
        public IMainEntitiesRepository Infrastructure { get; private set; } = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public IEnumerable<IMainEntitiesRepository> InfrastructureRepositories { get; set; }
        public EntityElementType ElementType { get; set; }
        public TreeRepository(Guid parentGuid) : base(parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
        }
        public TreeRepository(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
            Guid = guid;
        }
        private void Initialize()
        {
            Name = "Новый репозиторий";
            Childs = new List<IMainEntity>();
        }
    }
}
