using Philadelphus.Core.Domain.Entities.ElementsProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITypedModel
    {
        public EntityElementTypeModel ElementType { get; set; }
    }
}
