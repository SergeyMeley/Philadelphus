using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent
{
    public class EntityAttribute : RepositoryElementBase
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<EntityAttributeValue> Values { get; set; } = new List<EntityAttributeValue>();
        public EntityAttribute(Guid guid, IHavingChilds parent) : base(guid, parent)
        {

        }
    }
}
