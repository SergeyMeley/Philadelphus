using System.Threading.Tasks;

namespace Philadelphus.Presentation.Services.Interfaces
{
    public interface IMessageDialogService
    {
        void ShowInformation(string message, string title);

        void ShowWarning(string message, string title);

        void ShowError(string message, string title);

        // Async-версии (дефолт — обёртка поверх синхронных; Avalonia переопределяет на true-async).
        Task ShowInformationAsync(string message, string title)
        {
            ShowInformation(message, title);
            return Task.CompletedTask;
        }

        Task ShowWarningAsync(string message, string title)
        {
            ShowWarning(message, title);
            return Task.CompletedTask;
        }

        Task ShowErrorAsync(string message, string title)
        {
            ShowError(message, title);
            return Task.CompletedTask;
        }
    }
}
