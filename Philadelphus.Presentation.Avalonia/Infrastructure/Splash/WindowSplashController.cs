using global::Avalonia.Threading;

using Philadelphus.Presentation.Avalonia.Views.Windows;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Splash
{
    internal sealed class WindowSplashController : ISplashController
    {
        private readonly SplashWindow _window;

        public WindowSplashController(SplashWindow window)
            => _window = window;

        public void SetStatus(string status)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _window.SetStatus(status);
                return;
            }

            Dispatcher.UIThread.Post(() => _window.SetStatus(status), DispatcherPriority.Background);
        }

        public void Close()
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _window.Close();
                return;
            }

            Dispatcher.UIThread.Post(_window.Close, DispatcherPriority.Background);
        }
    }
}
