using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Services.Implementations
{
    /// <summary>
    /// WPF-реализация IWindowService. Создаёт окна через реестр ViewModel→Window.
    /// </summary>
    public class WpfWindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _registry = new();

        public WpfWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Register<TViewModel, TWindow>()
            where TViewModel : ViewModelBase
            where TWindow : Window
            => _registry[typeof(TViewModel)] = typeof(TWindow);

        public void Show<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            var window = CreateWindow(viewModel);
            window.Topmost = true;
            window.Show();
            window.Activate();
            window.Topmost = false;
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            var window = CreateWindow(viewModel);
            return window.ShowDialog();
        }

        public void Close<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == viewModel)
                {
                    window.Close();
                    return;
                }
            }
        }

        public void Hide<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == viewModel)
                {
                    window.Hide();
                    return;
                }
            }
        }

        public void ShowOrActivate<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext is TViewModel)
                {
                    window.DataContext = viewModel;
                    window.Activate();
                    return;
                }
            }
            var newWindow = CreateWindow(viewModel);
            newWindow.Owner = Application.Current.MainWindow;
            newWindow.Show();
        }

        private Window CreateWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            if (!_registry.TryGetValue(typeof(TViewModel), out var windowType))
                throw new InvalidOperationException($"Тип окна для {typeof(TViewModel).Name} не зарегистрирован.");

            var window = (Window)_serviceProvider.GetService(windowType)
                ?? throw new InvalidOperationException($"DI не смог создать {windowType.Name}.");

            window.DataContext = viewModel;
            return window;
        }
    }
}
