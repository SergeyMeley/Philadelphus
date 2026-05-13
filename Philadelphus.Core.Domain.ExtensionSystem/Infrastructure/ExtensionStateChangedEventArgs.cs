namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Аргументы события изменения состояния расширения
    /// </summary>
    public class ExtensionStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Предыдущее состояние.
        /// </summary>
        public ExtensionState OldState { get; }

        /// <summary>
        /// Новое состояние.
        /// </summary>
        public ExtensionState NewState { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExtensionStateChangedEventArgs" />.
        /// </summary>
        /// <param name="oldState">Предыдущее состояние.</param>
        /// <param name="newState">Новое состояние.</param>
        public ExtensionStateChangedEventArgs(
            ExtensionState oldState,
            ExtensionState newState)
        {
            OldState = oldState;
            NewState = newState;
        }
    }
}
