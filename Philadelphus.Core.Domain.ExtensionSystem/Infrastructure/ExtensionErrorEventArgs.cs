namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Аргументы события расширения.
    /// </summary>
    public class ExtensionErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Расширение.
        /// </summary>
        public string ExtensionName { get; set; }

        /// <summary>
        /// Исключение.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
