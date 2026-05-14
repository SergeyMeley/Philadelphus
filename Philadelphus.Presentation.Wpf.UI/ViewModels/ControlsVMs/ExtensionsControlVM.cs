using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// ViewModel для управления расширениями
    /// </summary>
    public class ExtensionsControlVM : ControlBaseVM
    {
        private readonly IExtensionManager _extensionManager;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private ExtensionInstanceVM _selectedExtension;
        private string _statusMessage;
        private bool _isExecuting;
        private PhiladelphusRepositoryVM _repositoryVM;

        public PhiladelphusRepositoryVM RepositoryVM
        {
            get => _repositoryVM;
            set => SetProperty(ref _repositoryVM, value);
        }

        /// <summary>
        /// Выполняет операцию расширения.
        /// </summary>
        /// <returns>Коллекция полученных данных.</returns>
        public static ObservableCollection<ExtensionInstanceVM> Extensions { get; } = new ObservableCollection<ExtensionInstanceVM>();
        /// <summary>
        /// Последние операции.
        /// </summary>
        public ObservableCollection<OperationLog> RecentOperations { get; }

        public ExtensionInstanceVM SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                if (SetProperty(ref _selectedExtension, value))
                {
                    UpdateCanExecuteForCurrentElement();
                }
                OnPropertyChanged(nameof(SelectedExtension));
            }
        }

        public IMainEntityModel SelectedElement
        {
            get => _repositoryExplorerControlVM.SelectedRepositoryMember?.Model;
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        /// <summary>
        /// Команда выполнения операции расширения.
        /// </summary>
        public ICommand StartExtensionCommand { get; }
        /// <summary>
        /// Команда выполнения операции расширения.
        /// </summary>
        public ICommand StopExtensionCommand { get; }
        /// <summary>
        /// Команда выполнения операции расширения.
        /// </summary>
        public ICommand ExecuteExtensionCommand { get; }
        /// <summary>
        /// Команда выполнения операции главного окна.
        /// </summary>
        public ICommand OpenMainWindowCommand { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExtensionsControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="extensionManager">Параметр extensionManager.</param>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ExtensionsControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IExtensionManager extensionManager,
            RepositoryExplorerControlVM repositoryExplorerControlVM,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(extensionManager);

            _extensionManager = extensionManager;
            _repositoryExplorerControlVM = repositoryExplorerControlVM;

            RecentOperations = new ObservableCollection<OperationLog>();

            StartExtensionCommand = new AsyncRelayCommand(ExecuteStartExtension, _ => SelectedExtension != null && SelectedExtension.State != ExtensionState.Running);
            StopExtensionCommand = new AsyncRelayCommand(ExecuteStopExtension, _ => SelectedExtension != null && SelectedExtension.State == ExtensionState.Running);
            ExecuteExtensionCommand = new AsyncRelayCommand(ExecuteMainMethod, _ => CanExecuteMainMethod());
            OpenMainWindowCommand = new RelayCommand(ExecuteOpenMainWindow, _ => SelectedExtension != null && SelectedExtension.State == ExtensionState.Running);
        }

        /// <summary>
        /// Инициализирует данные InitializeAsync.
        /// </summary>
        /// <param name="pluginsFolderPaths">Параметр pluginsFolderPaths.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task InitializeAsync(IEnumerable<DirectoryInfo> pluginsFolderPaths)
        {
            if (Extensions.Count > 0
                || _extensionManager.GetExtensions()?.Count > 0)
                return;
            foreach (var path in pluginsFolderPaths)
            {
                try
                {
                    // Загружаем расширения из DLL
                    await _extensionManager.LoadExtensionsAsync(path.FullName);
                    Debug.WriteLine($"Загружено расширений: {_extensionManager.GetExtensions().Count}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки расширений: {ex.Message}");
                    StatusMessage = $"Ошибка загрузки расширений: {ex.Message}";
                }
            }

            // Добавляем расширения в коллекцию ViewModel
            foreach (var ext in _extensionManager.GetExtensions())
            {
                var vm = new ExtensionInstanceVM(ext);
                Extensions.Add(vm);
            }

            // Автозагрузка расширений с AutoStart = true
            await _extensionManager.AutoStartExtensionsAsync();

            if (Extensions.Count > 0)
            {
                SelectedExtension = Extensions[0];
                StatusMessage = $"Загружено расширений: {Extensions.Count}";
            }
            else
            {
                StatusMessage = "Расширения не найдены";
            }
        }

        /// <summary>
        /// Инициализирует данные InitializeAsync.
        /// </summary>
        /// <param name="extensions">Параметр extensions.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task InitializeAsync(IEnumerable<ExtensionInstance> extensions)
        {
            foreach (var ext in extensions)
            {
                var vm = new ExtensionInstanceVM(ext);
                Extensions.Add(vm);
            }

            await _extensionManager.AutoStartExtensionsAsync();

            if (Extensions.Count > 0)
            {
                SelectedExtension = Extensions[0];
            }
        }

        private async Task ExecuteStartExtension(object parameter)
        {
            if (SelectedExtension == null) 
                return;

            try
            {
                IsExecuting = true;
                var originalExt = FindOriginalExtension(SelectedExtension);
                if (originalExt != null)
                {
                    await _extensionManager.StartExtensionAsync(originalExt);
                    StatusMessage = $"Расширение '{SelectedExtension.Name}' запущено";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при запуске: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка запуска", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private async Task ExecuteStopExtension(object parameter)
        {
            if (SelectedExtension == null) return;

            try
            {
                IsExecuting = true;
                var originalExt = FindOriginalExtension(SelectedExtension);
                if (originalExt != null)
                {
                    await _extensionManager.StopExtensionAsync(originalExt);
                    StatusMessage = $"Расширение '{SelectedExtension.Name}' остановлено";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при останове: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка останова", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private async Task ExecuteMainMethod(object parameter)
        {
            if (SelectedExtension == null) return;   // Убрал  "|| SelectedElement == null"

            try
            {
                IsExecuting = true;
                var originalExt = FindOriginalExtension(SelectedExtension);
                if (originalExt != null)
                {
                    await _extensionManager.ExecuteExtensionAsync(originalExt, _serviceProvider.GetRequiredService<IPhiladelphusRepositoryService>(), SelectedElement);
                    StatusMessage = $"Метод расширения '{SelectedExtension.Name}' успешно выполнен";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при выполнении: {ex.Message}";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private void ExecuteOpenMainWindow(object parameter)
        {
            if (SelectedExtension?.Window is Window window)
            {
                if (window.IsVisible == false)
                    window.Show();
                else
                    window.Hide();
            }
        }

        private bool CanExecuteMainMethod()
        {
            UpdateCanExecuteForCurrentElement();
            return SelectedExtension != null 
                && SelectedExtension.State == ExtensionState.Running 
                && SelectedExtension.CanExecute;
        }

        private async void UpdateCanExecuteForCurrentElement()
        {
            if (SelectedExtension != null && SelectedElement != null)
            {
                await SelectedExtension.UpdateCanExecuteAsync(SelectedElement);
            }
        }

        private ExtensionInstance FindOriginalExtension(ExtensionInstanceVM vmExt)
        {
            return _extensionManager.GetExtensions()
                .FirstOrDefault(e => e.Metadata.Name == vmExt.Name);
        }
    }
}
