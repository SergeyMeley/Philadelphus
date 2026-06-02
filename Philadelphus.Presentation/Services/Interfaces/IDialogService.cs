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
    }
}
