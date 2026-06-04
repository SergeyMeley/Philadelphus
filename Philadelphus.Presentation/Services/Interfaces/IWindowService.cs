using Philadelphus.Presentation.Wpf.UI.ViewModels;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Задаёт контракт для создания и управления окнами приложения.
    /// Заменяет прямое создание Window-экземпляров в ViewModels.
    /// </summary>
    public interface IWindowService
    {
        void Show<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        void Close<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        void Hide<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
        void ShowOrActivate<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase;
    }
}
