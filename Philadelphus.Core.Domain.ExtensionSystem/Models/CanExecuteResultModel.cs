namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Результат проверки возможности выполнения
    /// </summary>
    public class CanExecuteResultModel
    {
        public bool CanExecute { get; set; }
        public string Message { get; set; }

        public CanExecuteResultModel(bool canExecute, string message = "")
        {
            CanExecute = canExecute;
            Message = message;
        }
    }

}
