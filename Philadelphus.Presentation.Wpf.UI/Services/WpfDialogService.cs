using Philadelphus.Presentation.Services.Interfaces;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services
{
    /// <summary>
    /// WPF-реализация IDialogService через System.Windows.MessageBox.
    /// </summary>
    public class WpfDialogService : IDialogService
    {
        public void ShowError(string message, string title = "Ошибка")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public void ShowWarning(string message, string title = "Предупреждение")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public void ShowInformation(string message, string title = "Информация")
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public bool Confirm(string message, string title = "Подтверждение")
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
