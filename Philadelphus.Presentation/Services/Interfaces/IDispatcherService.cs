namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Задаёт контракт для маршалинга вызовов на UI-поток.
    /// Заменяет прямые обращения к Dispatcher/Application.Current.Dispatcher в ViewModels.
    /// </summary>
    public interface IDispatcherService
    {
        void Invoke(Action action);
        Task InvokeAsync(Action action);
        void BeginInvoke(Action action);
    }
}
