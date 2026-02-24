using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public abstract class MainEntityBase : IMainEntity
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? Sequence { get; set; }
        public string? Alias { get; set; }
        public string? CustomCode { get; set; }
        public bool IsHidden { get; set; }
        public AuditInfo AuditInfo { get; set; } = new AuditInfo();
        public MainEntityBase()
        {

        }
    }
}
