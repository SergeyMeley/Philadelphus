using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    /// <summary>
    /// Задает контракт для работы с основной сущностью.
    /// </summary>
    public interface IMainEntity
    {
        /// <summary>
        /// Уникальный идентификатор.
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание
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
        /// Скрыт от нового использования (устаревший для бизнеса)
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Информация для аудита
        /// </summary>
        public AuditInfo AuditInfo { get; set; }
    }
}
