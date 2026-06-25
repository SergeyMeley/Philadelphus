using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Avalonia.Views.Dialogs;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IDialogService" /> поверх программного <see cref="MessageBox" />.
    /// Синхронная семантика обеспечивается мостом <see cref="UiSync" />.
    /// </summary>
    public class AvaloniaDialogService : IDialogService
    {
        public void ShowError(string message, string title = "Ошибка")
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        public void ShowWarning(string message, string title = "Предупреждение")
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        public void ShowInformation(string message, string title = "Информация")
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        public bool Confirm(string message, string title = "Подтверждение")
            => UiSync.RunSync(() => MessageBox.ConfirmAsync(Owner, title, message));

        private static Window? Owner
            => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }
}
