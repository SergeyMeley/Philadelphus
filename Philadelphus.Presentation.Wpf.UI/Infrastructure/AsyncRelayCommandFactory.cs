using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Wpf.UI.Infrastructure
{
    /// <summary>
    /// WPF-реализация <see cref="IAsyncRelayCommandFactory" />.
    /// Создает <see cref="AsyncRelayCommand" /> на основе CommandManager.RequerySuggested — поведение команд не меняется.
    /// </summary>
    public sealed class AsyncRelayCommandFactory : IAsyncRelayCommandFactory
    {
        /// <inheritdoc />
        public IAsyncRelayCommand Create(Func<object, Task> execute, Predicate<object>? canExecute = null)
            => canExecute is null
                ? new AsyncRelayCommand(execute)
                : new AsyncRelayCommand(execute, canExecute);
    }
}
