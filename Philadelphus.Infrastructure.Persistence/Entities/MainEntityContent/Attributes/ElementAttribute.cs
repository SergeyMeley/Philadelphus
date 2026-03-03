using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes
{
    public class ElementAttribute : MainEntityBase
    {
        public Guid DeclaringUuid { get; set; }
        public Guid OwnerUuid { get; set; }
        public Guid DeclaringOwnerUuid { get; set; }
        public Guid? ValueTypeUuid { get; set; }
        public Guid? ValueUuid { get; set; }
        public bool IsCollectionValue { get; set; }
        public Guid[]? ValuesUuids { get; set; }
        public int VisibilityId { get; set; }
        public int OverrideId { get; set; }

        public ElementAttribute()
        {
            
        }
    }
}
