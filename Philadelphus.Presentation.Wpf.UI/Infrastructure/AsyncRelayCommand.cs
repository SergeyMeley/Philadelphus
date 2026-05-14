using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.Infrastructure
{
    /// <summary>
    /// Команда выполнения операции AsyncRelayCommand.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, System.Threading.Tasks.Task> _execute;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AsyncRelayCommand" />.
        /// </summary>
        /// <param name="execute">Параметр execute.</param>
        /// <param name="canExecute">Признак возможности выполнения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public AsyncRelayCommand(Func<object, System.Threading.Tasks.Task> execute, Predicate<object> canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Признак возможности выполнения операции.
        /// </summary>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        /// <summary>
        /// Выполняет операцию Execute.
        /// </summary>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter)) return;

            _isExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await _execute(parameter);
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}
