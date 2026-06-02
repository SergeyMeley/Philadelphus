using Philadelphus.Presentation.Services.Interfaces;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services.Implementations
{
    /// <summary>
    /// WPF-реализация IDispatcherService через Application.Current.Dispatcher.
    /// </summary>
    public class WpfDispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
            => Application.Current.Dispatcher.Invoke(action);

        public Task InvokeAsync(Action action)
            => Application.Current.Dispatcher.InvokeAsync(action).Task;

        public void BeginInvoke(Action action)
            => Application.Current.Dispatcher.BeginInvoke(action);
    }
}
