using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent
{
    public class EntityAttributeValue : RepositoryElementBase, IHavingParent
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public EntityAttributeValue(Guid guid, IHavingChilds parent) : base(guid, parent)
        {

        }
    }
}
