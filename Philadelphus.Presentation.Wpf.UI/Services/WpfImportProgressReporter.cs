using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// WPF-реализация <see cref="IImportProgressReporter" /> поверх <see cref="ImportProgressWindow" />.
    /// Все вызовы маршалятся на UI-поток приложения.
    /// </summary>
    internal sealed class WpfImportProgressReporter : IImportProgressReporter
    {
        private readonly IServiceProvider _serviceProvider;
        private ImportProgressWindow? _window;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="WpfImportProgressReporter" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов для создания окна прогресса.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="serviceProvider" /> равен null.</exception>
        public WpfImportProgressReporter(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public void Begin(string header, string status)
            => OnUi(() =>
            {
                _window = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                _window.Initialize(header, status);
                _window.Show();
            });

        /// <inheritdoc />
        public void Report(string status) => OnUi(() => _window?.UpdateStatus(status));

        /// <inheritdoc />
        public void Complete(string status) => OnUi(() => _window?.Complete(status));

        /// <inheritdoc />
        public void Fail(string status) => OnUi(() => _window?.Fail(status));

        private static void OnUi(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.Invoke(action);
        }
    }
}
