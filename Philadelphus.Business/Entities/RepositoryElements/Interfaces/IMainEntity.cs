using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.Interfaces
{
    public interface IMainEntity : ILinkableByGuid
    {
        public abstract EntityTypes EntityType { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public AuditInfo AuditInfo { get; }
    }
}
