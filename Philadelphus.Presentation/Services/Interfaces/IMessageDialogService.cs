namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public interface IMessageDialogService
    {
        void ShowInformation(string message, string title);

        void ShowWarning(string message, string title);

        void ShowError(string message, string title);
    }
}
