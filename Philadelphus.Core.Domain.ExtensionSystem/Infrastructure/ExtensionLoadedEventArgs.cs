namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public class ExtensionLoadedEventArgs : EventArgs
    {
        public ExtensionInstance Extension { get; set; }
    }
}
