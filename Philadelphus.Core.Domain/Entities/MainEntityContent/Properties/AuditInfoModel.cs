using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Когда создал", Description = "Когда создал")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Кто создал
        /// </summary>
        [Display(Name = "Кто создал", Description = "Кто создал")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Когда изменил
        /// </summary>
        [Display(Name = "Когда изменил", Description = "Когда изменил")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Кто изменил
        /// </summary>
        [Display(Name = "Кто изменил", Description = "Кто изменил")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Когда изменил содержимое
        /// </summary>
        [Display(Name = "Когда изменил содержимое", Description = "Когда изменил содержимое")]
        public DateTime? ContentUpdatedAt { get; set; }

        /// <summary>
        /// Кто изменил содержимое
        /// </summary>
        [Display(Name = "Кто изменил содержимое", Description = "Кто изменил содержимое")]
        public string? ContentUpdatedBy { get; set; }

        /// <summary>
        /// Удалено
        /// </summary>
        [Display(Name = "Удалено", Description = "Удалено")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Когда удалил
        /// </summary>
        [Display(Name = "Когда удалил", Description = "Когда удалил")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Кто удалил
        /// </summary>
        [Display(Name = "Кто удалил", Description = "Кто удалил")]
        public string? DeletedBy { get; set; }
    }
}
