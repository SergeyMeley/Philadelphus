using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRoot : MainEntityBase, IIndependentInfrastructure
    {
        public override EntityTypes EntityType { get => EntityTypes.Root; }
        public InfrastructureRepositoryTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesRepository Infrastructure { get; private set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public RepositoryElementType ElementType { get; set; }
        public TreeRoot(Guid parentGuid) : base(parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
            ElementType = new RepositoryElementType(Guid);
        }
        public TreeRoot(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
            Guid = guid;
            ElementType = new RepositoryElementType(Guid);
        }
        private void Initialize()
        {
            Name = "Новый корень";
            Childs = new List<IMainEntity>();
        }
    }
}
