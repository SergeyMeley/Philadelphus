using global::Avalonia.Threading;

using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IDispatcherService" /> через <see cref="Dispatcher.UIThread" />.
    /// </summary>
    public class AvaloniaDispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
            => Dispatcher.UIThread.Invoke(action);

        public Task InvokeAsync(Action action)
            => Dispatcher.UIThread.InvokeAsync(action).GetTask();

        public void BeginInvoke(Action action)
            => Dispatcher.UIThread.Post(action);
    }
}
