using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.ElementProperties
{
    public class EntityElementType : MainEntityBase
    {
        public override EntityTypes EntityType => EntityTypes.RepositoryElementType;
        public EntityElementType(Guid guid, ITyped parent) : base(guid)
        {
            Name = "TEST TYPE";
        }

    }
}
