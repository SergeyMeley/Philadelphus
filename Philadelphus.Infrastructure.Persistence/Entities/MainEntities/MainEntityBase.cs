using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    /// <summary>
    /// Представляет объект основной сущности.
    /// </summary>
    public abstract class MainEntityBase : IMainEntity
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public Guid Uuid { get; set; }
        
        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Описание.
        /// </summary>
        public string? Description { get; set; }
       
        /// <summary>
        /// Порядковый номер.
        /// </summary>
        public long? Sequence { get; set; }
      
        /// <summary>
        /// Псевдоним.
        /// </summary>
        public string? Alias { get; set; }
      
        /// <summary>
        /// Пользовательский код.
        /// </summary>
        public string? CustomCode { get; set; }
       
        /// <summary>
        /// Признак скрытого элемента.
        /// </summary>
        public bool IsHidden { get; set; }
       
        /// <summary>
        /// Данные аудита.
        /// </summary>
        public AuditInfo AuditInfo { get; set; } = new AuditInfo();
       
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainEntityBase" />.
        /// </summary>
        public MainEntityBase()
        {

        }
    }
}
