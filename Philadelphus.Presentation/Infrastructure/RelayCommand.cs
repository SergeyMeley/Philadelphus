namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Платформо-нейтральная реализация IRelayCommand.
    /// Не зависит от WPF CommandManager — RaiseCanExecuteChanged() вызывается явно.
    /// WPF-проект сохраняет собственную реализацию с подпиской на CommandManager.RequerySuggested.
    /// </summary>
    public class RelayCommand : IRelayCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter)
            => _execute(parameter);

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
