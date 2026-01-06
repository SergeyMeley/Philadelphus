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
        Initialized,
        /// <summary>
        /// Изменена (не сохранена)
        /// </summary>
        Changed,
        /// <summary>
        /// Сохранена или загружена
        /// </summary>
        SavedOrLoaded,
        /// <summary>
        /// Помечена для мягкого удаления
        /// </summary>
        ForSoftDelete,
        /// <summary>
        /// Помечена для жесткого удаления
        /// </summary>
        ForHardDelete,
        /// <summary>
        /// Удалена (мягко)
        /// </summary>
        SoftDeleted,
    }
}
