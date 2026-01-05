namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Аргументы события изменения состояния расширения
    /// </summary>
    public class ExtensionStateChangedEventArgs : EventArgs
    {
        public ExtensionState OldState { get; }
        public ExtensionState NewState { get; }

        public ExtensionStateChangedEventArgs(ExtensionState oldState, ExtensionState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
