using Microsoft.Extensions.Logging;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.ExtensionSystem.Models;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.WpfApplication.ViewModels.ControlsVMs
{
    /// <summary>
    /// ViewModel для управления расширениями
    /// </summary>
    public class ExtensionsControlVM : ControlVM
    {
        private readonly IExtensionManager _extensionManager;
        private ExtensionInstanceVM _selectedExtension;
        private MainEntityBaseModel _selectedElement;
        private string _statusMessage;
        private bool _isExecuting;
        private TreeRepositoryVM _repositoryVM;

        public TreeRepositoryVM RepositoryVM
        {
            get => _repositoryVM;
            set => SetProperty(ref _repositoryVM, value);
        }

        public static ObservableCollection<ExtensionInstanceVM> Extensions { get; } = new ObservableCollection<ExtensionInstanceVM>();
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

        public MainEntityBaseModel SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (SetProperty(ref _selectedElement, value))
                {
                    UpdateCanExecuteForCurrentElement();
                }
            }
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

        public ICommand StartExtensionCommand { get; }
        public ICommand StopExtensionCommand { get; }
        public ICommand ExecuteExtensionCommand { get; }
        public ICommand OpenMainWindowCommand { get; }

        public ExtensionsControlVM(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IExtensionManager extensionManager)
            : base(serviceProvider, logger, notificationService)
        {
            _extensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));

            RecentOperations = new ObservableCollection<OperationLog>();

            StartExtensionCommand = new AsyncRelayCommand(ExecuteStartExtension, _ => SelectedExtension != null && SelectedExtension.State != ExtensionState.Running);
            StopExtensionCommand = new AsyncRelayCommand(ExecuteStopExtension, _ => SelectedExtension != null && SelectedExtension.State == ExtensionState.Running);
            ExecuteExtensionCommand = new AsyncRelayCommand(ExecuteMainMethod, _ => CanExecuteMainMethod());
            OpenMainWindowCommand = new RelayCommand(ExecuteOpenMainWindow, _ => false);
        }

        public async Task InitializeAsync(IEnumerable<string> pluginsFolderPaths)
        {
            if (_extensionManager.GetExtensions()?.Count > 0)
                return;
            foreach (var path in pluginsFolderPaths)
            {
                try
                {
                    // Загружаем расширения из DLL
                    await _extensionManager.LoadExtensionsAsync(path);
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
            if (SelectedExtension == null || SelectedElement == null) return;

            try
            {
                IsExecuting = true;
                var originalExt = FindOriginalExtension(SelectedExtension);
                if (originalExt != null)
                {
                    await _extensionManager.ExecuteExtensionAsync(originalExt, SelectedElement);
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
            if (SelectedExtension?.Widget is Window window)
            {
                window.ShowDialog();
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
