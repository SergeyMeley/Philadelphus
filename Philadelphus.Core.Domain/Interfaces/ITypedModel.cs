using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITypedModel
    {
        public EntityElementTypeModel ElementType { get; set; }
    }
}
