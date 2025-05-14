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
    /// <summary>
    /// OLD
    /// </summary>
    public class OLD_ElementAttribute : MainEntityBase
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<ElementAttributeValue> Values { get; set; } = new List<ElementAttributeValue>();
        public OLD_ElementAttribute(Guid guid, TreeRepositoryMemberBase owner) : base(guid)
        {

        }
    }
}
