using System.Threading.Tasks;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;

using Philadelphus.Presentation.Avalonia.Helpers;
using Philadelphus.Presentation.Avalonia.Views.Dialogs;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IMessageDialogService" /> поверх программного <see cref="MessageBox" />.
    /// </summary>
    public class AvaloniaMessageDialogService : IMessageDialogService
    {
        public void ShowInformation(string message, string title)
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        public void ShowWarning(string message, string title)
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        public void ShowError(string message, string title)
            => UiSync.RunSync(() => MessageBox.ShowAsync(Owner, title, message));

        // True-async (без UiSync).
        public Task ShowInformationAsync(string message, string title)
            => MessageBox.ShowAsync(Owner, title, message);

        public Task ShowWarningAsync(string message, string title)
            => MessageBox.ShowAsync(Owner, title, message);

        public Task ShowErrorAsync(string message, string title)
            => MessageBox.ShowAsync(Owner, title, message);

        private static Window? Owner
            => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }
}
