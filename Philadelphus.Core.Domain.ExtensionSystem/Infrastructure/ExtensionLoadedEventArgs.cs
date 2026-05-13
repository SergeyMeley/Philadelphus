namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Аргументы события расширения.
    /// </summary>
    public class ExtensionLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Расширение.
        /// </summary>
        public ExtensionInstance Extension { get; set; }
    }
}
