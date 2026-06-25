using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Wpf.UI.Infrastructure
{
    /// <summary>
    /// WPF-реализация <see cref="IRelayCommandFactory" />.
    /// Создает <see cref="RelayCommand" /> на основе CommandManager.RequerySuggested — поведение команд не меняется.
    /// </summary>
    public sealed class RelayCommandFactory : IRelayCommandFactory
    {
        /// <inheritdoc />
        public IRelayCommand Create(Action<object> execute, Func<object, bool>? canExecute = null)
            => canExecute is null
                ? new RelayCommand(execute)
                : new RelayCommand(execute, canExecute);
    }
}
