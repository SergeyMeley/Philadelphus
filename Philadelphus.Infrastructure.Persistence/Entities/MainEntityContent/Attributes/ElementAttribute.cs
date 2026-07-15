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
        /// Уникальный идентификатор материализованного результата вычисления формулы.
        /// </summary>
        /// <remarks>
        /// Поле хранится только для запросов к БД и построения отчетов. При загрузке доменной модели
        /// оно не является источником значения: приложение вычисляет <see cref="ValueFormula" /> заново.
        /// </remarks>
        public Guid? ValueUuid { get; set; }

        /// <summary>
        /// Формула одиночного значения — единственный источник значения в приложении.
        /// </summary>
        public string? ValueFormula { get; set; }
       
        /// <summary>
        /// Признак коллекции значений.
        /// </summary>
        public bool IsCollectionValue { get; set; }
       
        /// <summary>
        /// Уникальные идентификаторы значений коллекционного атрибута.
        /// </summary>
        /// <remarks>
        /// В отличие от одиночного значения, коллекции пока не имеют формульного представления.
        /// Поэтому до реализации формул коллекций это поле остается источником их значений и загружается в модель.
        /// Планируемый синтаксис формулы массива: <c>={[uuid1],[uuid2],...,[uuidn]}</c>, где каждый
        /// элемент массива может быть произвольной формулой, возвращающей лист. После реализации
        /// <see cref="ValuesUuids" /> останется только материализованным значением для запросов и отчетов.
        /// </remarks>
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
