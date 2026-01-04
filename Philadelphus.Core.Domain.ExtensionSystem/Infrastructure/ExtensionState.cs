namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Состояния расширения
    /// </summary>
    public enum ExtensionState
    {
        Created,    // Создано, но не запущено
        Running,    // Запущено
        Stopped,    // Остановлено
        Error       // Ошибка
    }
}
