using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface ITypedModel
    {
        public EntityElementTypeModel ElementType { get; set; }
    }
}
