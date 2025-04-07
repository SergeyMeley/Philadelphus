using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class EntityAttributeValue : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public EntityAttributeValue(Guid parentGuid) : base(parentGuid)
        {

        }
        public EntityAttributeValue(Guid guid, Guid parentGuid) : base(guid, parentGuid)
        {

        }
    }
}
