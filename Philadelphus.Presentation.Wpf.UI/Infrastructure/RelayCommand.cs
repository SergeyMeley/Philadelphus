using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.Infrastructure
{
    /// <summary>
    /// Команда выполнения операции RelayCommand.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RelayCommand" />.
        /// </summary>
        /// <param name="execute">Параметр execute.</param>
        /// <param name="canExecute">Признак возможности выполнения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Признак возможности выполнения операции.
        /// </summary>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        /// <summary>
        /// Выполняет операцию Execute.
        /// </summary>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
