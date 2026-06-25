using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Avalonia.Infrastructure
{
    /// <summary>
    /// Avalonia-реализация <see cref="IRelayCommandFactory" />. Создаёт <see cref="AvaloniaRelayCommand" />
    /// с переопросом доступности через <see cref="AvaloniaCommandManager" /> (аналог WPF — на CommandManager).
    /// </summary>
    public sealed class AvaloniaRelayCommandFactory : IRelayCommandFactory
    {
        /// <inheritdoc />
        public IRelayCommand Create(Action<object> execute, Func<object, bool>? canExecute = null)
            => canExecute is null
                ? new AvaloniaRelayCommand(execute)
                : new AvaloniaRelayCommand(execute, canExecute);
    }
}
