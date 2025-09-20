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
    public interface IMainEntityModel : ILinkableByGuidModel
    {
        public abstract EntityTypesModel EntityType { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public AuditInfoModel AuditInfo { get; }
        public State State { get; set; }
    }
}
