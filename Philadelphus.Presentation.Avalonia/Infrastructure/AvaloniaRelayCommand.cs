using Philadelphus.Presentation.Infrastructure;

namespace Philadelphus.Presentation.Avalonia.Infrastructure
{
    /// <summary>
    /// Avalonia-реализация <see cref="IRelayCommand" /> с переопросом доступности через
    /// <see cref="AvaloniaCommandManager" /> (аналог WPF RelayCommand на CommandManager.RequerySuggested).
    /// </summary>
    public class AvaloniaRelayCommand : IRelayCommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { AvaloniaCommandManager.RequerySuggested += value; }
            remove { AvaloniaCommandManager.RequerySuggested -= value; }
        }

        public AvaloniaRelayCommand(Action<object> execute, Func<object, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => AvaloniaCommandManager.RaiseRequerySuggested();
    }
}
