namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Платформо-нейтральная фабрика <see cref="IRelayCommand" /> по умолчанию.
    /// Создаёт <see cref="RelayCommand" /> (без WPF CommandManager — RaiseCanExecuteChanged()
    /// вызывается явно). WPF подставляет собственную фабрику на CommandManager; Avalonia и прочие
    /// платформы используют эту.
    /// </summary>
    public sealed class DefaultRelayCommandFactory : IRelayCommandFactory
    {
        /// <inheritdoc />
        public IRelayCommand Create(Action<object> execute, Func<object, bool>? canExecute = null)
            => canExecute is null
                ? new RelayCommand(execute)
                : new RelayCommand(execute, canExecute);
    }
}
