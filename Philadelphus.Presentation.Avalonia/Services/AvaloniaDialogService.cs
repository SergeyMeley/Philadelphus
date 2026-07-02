using System.Threading.Tasks;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

using Philadelphus.Presentation.Avalonia.Views.Dialogs;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IDialogService" /> поверх программного <see cref="MessageBox" />.
    /// </summary>
    public class AvaloniaDialogService : IDialogService
    {
        public void ShowError(string message, string title = "Ошибка")
            => ThrowSyncNotSupported();

        public void ShowWarning(string message, string title = "Предупреждение")
            => ThrowSyncNotSupported();

        public void ShowInformation(string message, string title = "Информация")
            => ThrowSyncNotSupported();

        public bool Confirm(string message, string title = "Подтверждение")
            => throw new NotSupportedException("Avalonia message dialogs are asynchronous. Use IDialogService.ConfirmAsync.");

        // True-async: напрямую возвращаем задачу MessageBox.
        public Task ShowErrorAsync(string message, string title = "Ошибка")
            => MessageBox.ShowAsync(Owner, title, message);

        public Task ShowWarningAsync(string message, string title = "Предупреждение")
            => MessageBox.ShowAsync(Owner, title, message);

        public Task ShowInformationAsync(string message, string title = "Информация")
            => MessageBox.ShowAsync(Owner, title, message);

        public Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
            => MessageBox.ConfirmAsync(Owner, title, message);

        private static Window? Owner
            => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        private static void ThrowSyncNotSupported()
            => throw new NotSupportedException("Avalonia message dialogs are asynchronous. Use IDialogService.*Async methods.");
    }
}
