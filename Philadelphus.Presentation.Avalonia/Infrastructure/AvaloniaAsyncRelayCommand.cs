using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Avalonia.Infrastructure
{
    /// <summary>
    /// Avalonia-реализация <see cref="IAsyncRelayCommand" /> с переопросом доступности через
    /// <see cref="AvaloniaCommandManager" /> (аналог WPF AsyncRelayCommand на CommandManager).
    /// </summary>
    public class AvaloniaAsyncRelayCommand : IAsyncRelayCommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object>? _canExecute;
        private bool _isExecuting;

        public bool IsExecuting => _isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { AvaloniaCommandManager.RequerySuggested += value; }
            remove { AvaloniaCommandManager.RequerySuggested -= value; }
        }

        public AvaloniaAsyncRelayCommand(Func<object, Task> execute, Predicate<object>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _isExecuting == false && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

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

        public void RaiseCanExecuteChanged() => AvaloniaCommandManager.RaiseRequerySuggested();
    }
}
