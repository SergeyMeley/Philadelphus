using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Состояние сущности
    /// </summary>
    public enum State
    {
        /// <summary>
        /// Инициализирована
        /// </summary>
        [Display(Name = "Н", Description = "Создан (инициализирован)")] 
        Initialized,
        /// <summary>
        /// Изменена (не сохранена)
        /// </summary>
        [Display(Name = "И", Description = "Изменен")]
        Changed,
        /// <summary>
        /// Сохранена или загружена
        /// </summary>
        [Display(Name = "С", Description = "Сохранен или загружен")]
        SavedOrLoaded,
        /// <summary>
        /// Помечена для мягкого удаления
        /// </summary>
        [Display(Name = "У", Description = "Помечен для удаления (мягкого)")]
        ForSoftDelete,
        /// <summary>
        /// Помечена для жесткого удаления
        /// </summary>
        [Display(Name = "Ж", Description = "Помечен для удаления (жесткого)")] 
        ForHardDelete,
        /// <summary>
        /// Удалена (мягко)
        /// </summary>
        [Display(Name = "М", Description = "Удален (мягко)")]
        SoftDeleted,
    }
}
