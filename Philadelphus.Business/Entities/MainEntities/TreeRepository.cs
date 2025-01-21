using Philadelphus.Business.Entities.Enums;
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
        public override InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesRepository Infrastructure { get; private set; } = new WindowsFileSystemRepository.Repositories.MainEntityRepository();
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public IEnumerable<TreeRoot> ChildTreeRoots { get; set; } = new List<TreeRoot>();
        public IEnumerable<IMainEntitiesRepository> InfrastructureRepositories { get; set; }
        public TreeRepository(Guid parentGuid) : base(parentGuid)
        {
            
        }
        public TreeRepository(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {

        }
    }
}
