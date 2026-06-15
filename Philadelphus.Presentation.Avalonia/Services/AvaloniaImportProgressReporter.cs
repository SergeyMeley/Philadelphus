using global::Avalonia.Threading;

using Microsoft.Extensions.DependencyInjection;

using Philadelphus.Presentation.Avalonia.Views.Windows;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IImportProgressReporter" /> поверх <see cref="ImportProgressWindow" />.
    /// Все вызовы маршалятся на UI-поток. Семантика повторяет WPF-реализацию.
    /// </summary>
    internal sealed class AvaloniaImportProgressReporter : IImportProgressReporter
    {
        private readonly IServiceProvider _serviceProvider;
        private ImportProgressWindow? _window;

        public AvaloniaImportProgressReporter(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        public void Begin(string header, string status)
            => OnUi(() =>
            {
                _window = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                _window.Initialize(header, status);
                _window.Show();
            });

        public void Report(string status) => OnUi(() => _window?.UpdateStatus(status));

        public void Complete(string status) => OnUi(() => _window?.Complete(status));

        public void Fail(string status) => OnUi(() => _window?.Fail(status));

        private static void OnUi(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
                return;
            }

            Dispatcher.UIThread.Invoke(action);
        }
    }
}
