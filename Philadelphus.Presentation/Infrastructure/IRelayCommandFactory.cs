namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Фабрика синхронных команд <see cref="IRelayCommand" />.
    /// Позволяет иметь платформенные реализации (WPF — на CommandManager, Avalonia — на shared RelayCommand)
    /// без прямого создания команд в моделях представления.
    /// </summary>
    public interface IRelayCommandFactory
    {
        /// <summary>
        /// Создает команду с указанным действием и необязательным условием доступности.
        /// </summary>
        /// <param name="execute">Действие, выполняемое командой.</param>
        /// <param name="canExecute">Условие доступности команды.</param>
        /// <returns>Созданная команда <see cref="IRelayCommand" />.</returns>
        IRelayCommand Create(Action<object> execute, Func<object, bool>? canExecute = null);
    }
}
