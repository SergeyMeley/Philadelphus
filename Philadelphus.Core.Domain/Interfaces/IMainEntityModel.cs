using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

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
