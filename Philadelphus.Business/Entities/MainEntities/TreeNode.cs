using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeNode : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Node; }
        public IMainEntitiesRepository Infrastructure { get; private set; }
        public IEnumerable<EntityAttributeEntry> AttributeEntries { get; set; } = new List<EntityAttributeEntry>();
        public EntityElementType ElementType { get; set; }
        public TreeNode(Guid parentGuid) : base(parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
        }
        public TreeNode(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
            Guid = guid;
        }
        private void Initialize()
        {
            Name = "Новый узел";
            Childs = new List<IMainEntity>();
        }
    }
}
