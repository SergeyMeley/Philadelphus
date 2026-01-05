using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes
{
    public class ElementAttribute : MainEntityBase
    {
        public Guid OwnerUuid { get; set; }
        public Guid? ValueTypeUuid { get; set; }
        public Guid? ValueUuid { get; set; }
        public ElementAttribute()
        {
            
        }
    }
}
