using Philadelphus.Business.Entities.ElementsProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    public interface ITypedModel
    {
        public EntityElementTypeModel ElementType { get; set; }
    }
}
