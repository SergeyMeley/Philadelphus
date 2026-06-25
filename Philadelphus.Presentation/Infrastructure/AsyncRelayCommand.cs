namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Платформо-нейтральная реализация IAsyncRelayCommand.
    /// Не зависит от WPF CommandManager — RaiseCanExecuteChanged() вызывается явно.
    /// </summary>
    public class AsyncRelayCommand : IAsyncRelayCommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object>? _canExecute;
        private bool _isExecuting;

        public bool IsExecuting => _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
            => _isExecuting == false && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
