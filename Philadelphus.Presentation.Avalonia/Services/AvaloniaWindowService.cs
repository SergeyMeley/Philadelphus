using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.ApplicationLifetimes;
using global::Avalonia.Threading;

using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;

namespace Philadelphus.Presentation.Avalonia.Services
{
    /// <summary>
    /// Avalonia-реализация <see cref="IWindowService" />. Создаёт окна через реестр ViewModel→Window
    /// и оперирует списком открытых окон рабочего стола (<see cref="IClassicDesktopStyleApplicationLifetime.Windows" />).
    /// Семантика повторяет WPF-реализацию.
    /// </summary>
    public class AvaloniaWindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Type> _registry = new();

        public AvaloniaWindowService(IServiceProvider serviceProvider)
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

            void ShowCore()
            {
                // Владельцем берём видимое окно: Avalonia запрещает ShowDialog с невидимым
                // владельцем (например скрытым LaunchWindow).
                var owner = ResolveVisibleOwner(window);
                if (owner is not null)
                {
                    // Модально, но без синхронного ожидания закрытия: Avalonia ShowDialog
                    // асинхронный, а блокировка UI-потока (busy-loop) вешает обработку ввода окна.
                    // Результат диалога сейчас никем не используется.
                    _ = window.ShowDialog(owner);
                }
                else
                {
                    window.Show();
                }
            }

            if (Dispatcher.UIThread.CheckAccess())
            {
                ShowCore();
            }
            else
            {
                Dispatcher.UIThread.Post(ShowCore);
            }

            return null;
        }

        public void Close<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            foreach (var window in Windows)
            {
                if (ReferenceEquals(window.DataContext, viewModel))
                {
                    window.Close();
                    return;
                }
            }
        }

        public void Hide<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            // Сначала ищем по зарегистрированному типу окна (как Show/CreateWindow): VM может быть
            // transient, и переданный экземпляр не обязан совпадать по ссылке с DataContext окна.
            if (_registry.TryGetValue(typeof(TViewModel), out var windowType))
            {
                foreach (var window in Windows)
                {
                    if (windowType.IsInstanceOfType(window))
                    {
                        window.Hide();
                        return;
                    }
                }
            }

            foreach (var window in Windows)
            {
                if (ReferenceEquals(window.DataContext, viewModel))
                {
                    window.Hide();
                    return;
                }
            }
        }

        public void ShowOrActivate<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            foreach (var window in Windows)
            {
                if (window.DataContext is TViewModel)
                {
                    window.DataContext = viewModel;
                    window.Activate();
                    return;
                }
            }

            var newWindow = CreateWindow(viewModel);
            newWindow.Show();
        }

        public void ToggleVisibility(object? platformWindow)
        {
            if (platformWindow is Window window)
            {
                if (window.IsVisible)
                {
                    window.Hide();
                }
                else
                {
                    window.Show();
                }
            }
        }

        private Window CreateWindow<TViewModel>(TViewModel viewModel) where TViewModel : ViewModelBase
        {
            if (_registry.TryGetValue(typeof(TViewModel), out var windowType) == false)
            {
                throw new InvalidOperationException($"Тип окна для {typeof(TViewModel).Name} не зарегистрирован.");
            }

            var window = (Window?)_serviceProvider.GetService(windowType)
                ?? throw new InvalidOperationException($"DI не смог создать {windowType.Name}.");

            window.DataContext = viewModel;
            AttachViewModelLifecycle(window, viewModel);
            return window;
        }

        private static void AttachViewModelLifecycle(Window window, ViewModelBase viewModel)
        {
            EventHandler? closeRequestedHandler = null;
            if (viewModel is IWindowCloseRequestSource closeRequestSource)
            {
                closeRequestedHandler = (_, _) => window.Close();
                closeRequestSource.CloseRequested += closeRequestedHandler;
            }

            window.Closed += (_, _) =>
            {
                if (viewModel is IWindowCloseRequestSource closeRequestSource
                    && closeRequestedHandler != null)
                {
                    closeRequestSource.CloseRequested -= closeRequestedHandler;
                }

                if (viewModel is IDisposable disposable)
                    disposable.Dispose();
            };
        }

        /// <summary>
        /// Подбирает видимое окно-владельца для модального показа: приоритет — MainWindow рабочего стола,
        /// если он видим; иначе последнее видимое окно. Возвращает null, если видимых окон нет.
        /// </summary>
        private static Window? ResolveVisibleOwner(Window window)
        {
            var main = Desktop?.MainWindow;
            if (main is { IsVisible: true } && ReferenceEquals(main, window) == false)
            {
                return main;
            }

            return Windows.LastOrDefault(w => w.IsVisible && ReferenceEquals(w, window) == false);
        }

        private static IClassicDesktopStyleApplicationLifetime? Desktop
            => Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        private static IReadOnlyList<Window> Windows
            => Desktop?.Windows ?? Array.Empty<Window>();
    }
}
