using System.Threading.Tasks;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Задаёт контракт для показа диалоговых сообщений.
    /// Заменяет прямые вызовы MessageBox.Show() в ViewModels.
    /// </summary>
    public interface IDialogService
    {
        void ShowError(string message, string title = "Ошибка");
        void ShowWarning(string message, string title = "Предупреждение");
        void ShowInformation(string message, string title = "Информация");
        bool Confirm(string message, string title = "Подтверждение");

        // Async-версии. По умолчанию оборачивают синхронные (для WPF — модально-синхронно).
        // Платформа может переопределить на нативно-асинхронные (Avalonia — без блокировки UI-потока).
        Task ShowErrorAsync(string message, string title = "Ошибка")
        {
            ShowError(message, title);
            return Task.CompletedTask;
        }

        Task ShowWarningAsync(string message, string title = "Предупреждение")
        {
            ShowWarning(message, title);
            return Task.CompletedTask;
        }

        Task ShowInformationAsync(string message, string title = "Информация")
        {
            ShowInformation(message, title);
            return Task.CompletedTask;
        }

        Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
            => Task.FromResult(Confirm(message, title));
    }
}
