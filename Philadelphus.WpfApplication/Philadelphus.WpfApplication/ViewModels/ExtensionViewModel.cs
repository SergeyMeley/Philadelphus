using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Philadelphus.WpfApplication.ViewModels
{
    /// <summary>
    /// ViewModel для управления расширениями
    /// </summary>
    public class ExtensionsViewModel : ViewModelBase
    {
        private readonly IExtensionManager _extensionManager;
        private ExtensionInstance _selectedExtension;
        private TreeRepositoryMemberBaseModel _selectedElement;
        private string _statusMessage;
        private bool _isExecuting;

        public TreeRepositoryVM RepositoryViewModel { get; set; }
        public ObservableCollection<ExtensionInstance> Extensions { get; }
        public ObservableCollection<OperationLog> RecentOperations { get; }

        public ExtensionInstance SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                if (SetProperty(ref _selectedExtension, value))
                {
                    UpdateCanExecuteForCurrentElement();
                }
            }
        }

        public TreeRepositoryMemberBaseModel SelectedElement
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

        // Команды
        public ICommand StartExtensionCommand { get; }
        public ICommand StopExtensionCommand { get; }
        public ICommand ExecuteExtensionCommand { get; }
        public ICommand OpenMainWindowCommand { get; }

        public ExtensionsViewModel(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager ?? throw new ArgumentNullException(nameof(extensionManager));
            Extensions = new ObservableCollection<ExtensionInstance>();
            RecentOperations = new ObservableCollection<OperationLog>();

            StartExtensionCommand = new AsyncRelayCommand(ExecuteStartExtension, _ => SelectedExtension != null && SelectedExtension.State != ExtensionState.Running);
            StopExtensionCommand = new AsyncRelayCommand(ExecuteStopExtension, _ => SelectedExtension != null && SelectedExtension.State == ExtensionState.Running);
            ExecuteExtensionCommand = new AsyncRelayCommand(ExecuteMainMethod, _ => CanExecuteMainMethod());
            OpenMainWindowCommand = new RelayCommand(ExecuteOpenMainWindow, _ => SelectedExtension?.Extension.GetMainWindow() != null);
        }

        public async Task InitializeAsync(ObservableCollection<ExtensionInstance> extensions)
        {
            foreach (var ext in extensions)
            {
                Extensions.Add(ext);
                ext.PropertyChanged += Extension_PropertyChanged;
            }

            // Автозагрузка расширений с AutoStart = true
            await _extensionManager.AutoStartExtensionsAsync();

            if (Extensions.Count > 0)
            {
                SelectedExtension = Extensions[0];
            }
        }

        private async Task ExecuteStartExtension(object parameter)
        {
            if (SelectedExtension == null) return;

            try
            {
                IsExecuting = true;
                await _extensionManager.StartExtensionAsync(SelectedExtension);
                StatusMessage = $"Расширение '{SelectedExtension.Metadata.Name}' запущено";
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
                await _extensionManager.StopExtensionAsync(SelectedExtension);
                StatusMessage = $"Расширение '{SelectedExtension.Metadata.Name}' остановлено";
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
                var result = await _extensionManager.ExecuteExtensionAsync(SelectedExtension, SelectedElement);
                StatusMessage = $"Метод расширения '{SelectedExtension.Metadata.Name}' успешно выполнен";

                // Обновляем информацию об операции
                UpdateOperationsLog(SelectedExtension);
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
            if (SelectedExtension?.Extension.GetMainWindow() is Window window)
            {
                window.ShowDialog();
            }
        }

        private bool CanExecuteMainMethod()
        {
            if (SelectedExtension == null || SelectedElement == null)
                return false;

            return SelectedExtension.State == ExtensionState.Running &&
                   SelectedExtension.LastCanExecuteResultModel.CanExecute;
        }

        private async void UpdateCanExecuteForCurrentElement()
        {
            if (SelectedExtension != null && SelectedElement != null)
            {
                await SelectedExtension.UpdateCanExecuteAsync(SelectedElement);
                UpdateOperationsLog(SelectedExtension);
            }
        }

        private void UpdateOperationsLog(ExtensionInstance extension)
        {
            RecentOperations.Clear();

            // Показываем последние 10 операций
            var recent = extension.OperationHistory.Skip(Math.Max(0, extension.OperationHistory.Count - 10)).ToList();
            foreach (var log in recent)
            {
                RecentOperations.Add(log);
            }
        }

        private void Extension_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender == SelectedExtension && e.PropertyName == nameof(ExtensionInstance.OperationHistory))
            {
                UpdateOperationsLog(SelectedExtension);
            }
        }
    }
}
