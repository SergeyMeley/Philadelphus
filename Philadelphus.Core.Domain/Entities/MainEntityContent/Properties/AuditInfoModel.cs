namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Properties
{
    /// <summary>
    /// Информация для аудите
    /// </summary>
    public class AuditInfoModel
    {
        /// <summary>
        /// Когда создал
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Кто создал
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Когда обновил
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Кто обновил
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Когда обновил содержимое
        /// </summary>
        public DateTime? ContentUpdatedAt { get; set; }

        /// <summary>
        /// Кто обновил содержимое
        /// </summary>
        public string? ContentUpdatedBy { get; set; }

        /// <summary>
        /// Удалено
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Когда удалил
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Кто удалил
        /// </summary>
        public string? DeletedBy { get; set; }
    }
}
