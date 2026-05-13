namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties
{
    /// <summary>
    /// Представляет объект информации аудита.
    /// </summary>
    public class AuditInfo
    {
        /// <summary>
        /// Когда создал.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Кто создал.
        /// </summary>
        public string CreatedBy { get; set; }
        
        /// <summary>
        /// Когда изменил.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
       
        /// <summary>
        /// Кто изменил.
        /// </summary>
        public string? UpdatedBy { get; set; }
        
        /// <summary>
        /// Удален.
        /// </summary>
        public bool IsDeleted { get; set; }
       
        /// <summary>
        /// Когда удалил.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
     
        /// <summary>
        /// Кто удалил.
        /// </summary>
        public string? DeletedBy { get; set; }
    }
}
