using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;

namespace Philadelphus.Core.Domain.Interfaces
{
    //TODO: Удалить, устарело
    /// <summary>
    /// Тип сущности
    /// </summary>
    public interface ITypedModel
    {
        /// <summary>
        /// Тип
        /// </summary>
        public EntityElementTypeModel ElementType { get; set; }
    }
}
