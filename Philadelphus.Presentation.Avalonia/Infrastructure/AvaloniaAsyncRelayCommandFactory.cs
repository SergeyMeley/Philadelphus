using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Avalonia.Infrastructure
{
    /// <summary>
    /// Avalonia-реализация <see cref="IAsyncRelayCommandFactory" />. Создаёт <see cref="AvaloniaAsyncRelayCommand" />
    /// с переопросом доступности через <see cref="AvaloniaCommandManager" />.
    /// </summary>
    public sealed class AvaloniaAsyncRelayCommandFactory : IAsyncRelayCommandFactory
    {
        /// <inheritdoc />
        public IAsyncRelayCommand Create(Func<object, Task> execute, Predicate<object>? canExecute = null)
            => canExecute is null
                ? new AvaloniaAsyncRelayCommand(execute)
                : new AvaloniaAsyncRelayCommand(execute, canExecute);
    }
}
