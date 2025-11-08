using Philadelphus.Business.Entities.ElementsContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    public interface IHavingAttributesModel
    {
        public Guid Guid { get; }
        public IEnumerable<ElementAttributeModel> PersonalAttributes { get; }
        public IEnumerable<ElementAttributeModel> ParentElementAttributes { get; }
        public bool HasContent { get; set; }
    }
}
