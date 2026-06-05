using IRelayCommand = Philadelphus.Presentation.Infrastructure.IRelayCommand;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Задаёт контракт для доступа к командам приложения из ViewModels.
    /// Позволяет перенести ControlBaseVM и его наследников в shared Presentation
    /// без прямой зависимости от WPF-реализации ApplicationCommandsVM.
    /// </summary>
    public interface IApplicationCommandsVM
    {
        IRelayCommand OpenMainWindowCommand { get; }
        IRelayCommand OpenLaunchWindowCommand { get; }
        IRelayCommand OpenFormulaEditorWindowCommand { get; }
        IRelayCommand OpenDataStoragesSettingsControlCommand { get; }
    }
}
