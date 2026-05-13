namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Запись в журнал операций
    /// </summary>
    public class OperationLog
    {
        /// <summary>
        /// Время события.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Операция.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Подробные сведения.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Признак ошибки.
        /// </summary>
        public bool IsError { get; set; }
    }
}
