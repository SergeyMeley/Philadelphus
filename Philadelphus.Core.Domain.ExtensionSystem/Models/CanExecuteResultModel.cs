namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Результат проверки возможности выполнения
    /// </summary>
    public class CanExecuteResultModel
    {
        /// <summary>
        /// Признак возможности выполнения операции.
        /// </summary>
        public bool CanExecute { get; set; }

        /// <summary>
        /// Сообщение.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CanExecuteResultModel" />.
        /// </summary>
        /// <param name="canExecute">Признак возможности выполнения.</param>
        /// <param name="message">Сообщение.</param>
        public CanExecuteResultModel(
            bool canExecute, 
            string message = "")
        {
            CanExecute = canExecute;
            Message = message;
        }
    }

}
