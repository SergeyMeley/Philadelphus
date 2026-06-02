using System.Windows.Input;

namespace Philadelphus.Presentation.Infrastructure
{
    /// <summary>
    /// Расширяет ICommand возможностью явного обновления CanExecute.
    /// Позволяет иметь разные реализации для WPF (CommandManager) и Avalonia (ручной RaiseCanExecuteChanged).
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
