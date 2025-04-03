using Philadelphus.Business.Entities.Enums;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class RepositoryElementType : MainEntityBase
    {
        public override EntityTypes EntityType => EntityTypes.RepositoryElementType;
        public RepositoryElementType(Guid parentGuid) : base(parentGuid)
        {
            Name = "TEST TYPE";
        }

    }
}
