using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Представляет объект атрибута элемента.
    /// </summary>
    public class ElementAttribute : WorkingTreeMemberBase
    {
        /// <summary>
        /// Уникальный идентификатор объявления.
        /// </summary>
        public Guid DeclaringUuid { get; set; }
        
        /// <summary>
        /// Уникальный идентификатор владельца.
        /// </summary>
        public Guid OwnerUuid { get; set; }
        
        /// <summary>
        /// Уникальный идентификатор владельца объявления.
        /// </summary>
        public Guid DeclaringOwnerUuid { get; set; }
        
        /// <summary>
        /// Уникальный идентификатор типа значения.
        /// </summary>
        public Guid? ValueTypeUuid { get; set; }
       
        /// <summary>
        /// Уникальный идентификатор значения.
        /// </summary>
        public Guid? ValueUuid { get; set; }
       
        /// <summary>
        /// Признак коллекции значений.
        /// </summary>
        public bool IsCollectionValue { get; set; }
       
        /// <summary>
        /// Уникальные идентификаторы значений.
        /// </summary>
        public Guid[]? ValuesUuids { get; set; }
        
        /// <summary>
        /// Идентификатор области видимости.
        /// </summary>
        public int VisibilityId { get; set; }
        
        /// <summary>
        /// Идентификатор режима переопределения.
        /// </summary>
        public int OverrideId { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ElementAttribute" />.
        /// </summary>
        public ElementAttribute()
        {
            
        }
    }
}
