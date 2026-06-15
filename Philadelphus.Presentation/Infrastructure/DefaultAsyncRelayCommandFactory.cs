namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Платформо-нейтральная фабрика <see cref="IAsyncRelayCommand" /> по умолчанию.
    /// Создаёт <see cref="AsyncRelayCommand" /> (без WPF CommandManager — RaiseCanExecuteChanged()
    /// вызывается явно). WPF подставляет собственную фабрику на CommandManager; Avalonia и прочие
    /// платформы используют эту.
    /// </summary>
    public sealed class DefaultAsyncRelayCommandFactory : IAsyncRelayCommandFactory
    {
        /// <inheritdoc />
        public IAsyncRelayCommand Create(Func<object, Task> execute, Predicate<object>? canExecute = null)
            => canExecute is null
                ? new AsyncRelayCommand(execute)
                : new AsyncRelayCommand(execute, canExecute);
    }
}
