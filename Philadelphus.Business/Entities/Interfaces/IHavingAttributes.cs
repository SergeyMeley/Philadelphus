using Philadelphus.Business.Entities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Interfaces
{
    public interface IHavingAttributes
    {
        public Guid Guid { get; }
        public IEnumerable<EntityAttributeEntry> PersonalAttributes { get; }
        public IEnumerable<EntityAttributeEntry> ParentElementAttributes { get; }
        public bool HasContent { get; set; }
    }
}
