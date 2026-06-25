namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Фабрика асинхронных команд <see cref="IAsyncRelayCommand" />.
    /// Платформенные реализации создают соответствующую платформе команду
    /// без прямого создания команд в моделях представления.
    /// </summary>
    public interface IAsyncRelayCommandFactory
    {
        /// <summary>
        /// Создает асинхронную команду с указанным действием и необязательным условием доступности.
        /// </summary>
        /// <param name="execute">Асинхронное действие, выполняемое командой.</param>
        /// <param name="canExecute">Условие доступности команды.</param>
        /// <returns>Созданная команда <see cref="IAsyncRelayCommand" />.</returns>
        IAsyncRelayCommand Create(Func<object, Task> execute, Predicate<object>? canExecute = null);
    }
}
