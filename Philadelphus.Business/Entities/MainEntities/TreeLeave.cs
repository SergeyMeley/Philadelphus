using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeLeave : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public string Uuid { get; set; }
        public string Type { get; set; }
        public IEnumerable<AttributeEntry> AttributeEntries { get; set; } = new List<AttributeEntry>();
        public TreeLeave(Guid parentGuid) : base(parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
        }
        public TreeLeave(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {
            Initialize();
            ParentGuid = parentGuid;
            Guid = guid;
        }
        private void Initialize()
        {
            Name = "Новый лист";
            Childs = new List<IMainEntity>();
        }
    }
}
