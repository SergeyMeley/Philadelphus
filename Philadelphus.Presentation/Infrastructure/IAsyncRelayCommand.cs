namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Расширяет IRelayCommand поддержкой асинхронного выполнения.
    /// </summary>
    public interface IAsyncRelayCommand : IRelayCommand
    {
        bool IsExecuting { get; }
    }
}
