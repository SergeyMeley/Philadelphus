using Philadelphus.Core.Domain.Entities.ElementsProperties;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface IMainEntityModel : ILinkableByUuidModel
    {
        public abstract EntityTypesModel EntityType { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Alias { get; set; }
        public string CustomCode { get; set; }
        public AuditInfoModel AuditInfo { get; }
        public State State { get; }
        public IMainEntity DbEntity { get; }
        public IDataStorageModel DataStorage { get; }
    }
}
