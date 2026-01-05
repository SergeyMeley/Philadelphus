namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public class ExtensionErrorEventArgs : EventArgs
    {
        public string ExtensionName { get; set; }
        public Exception Exception { get; set; }
    }
}
