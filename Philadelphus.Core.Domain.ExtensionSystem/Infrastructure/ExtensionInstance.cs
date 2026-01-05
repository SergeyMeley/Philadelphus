using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Обертка вокруг расширения для управления состоянием
    /// </summary>
    public class ExtensionInstance : INotifyPropertyChanged
    {
        private ExtensionState _state;
        private CanExecuteResultModel _lastCanExecuteResultModel;
        private Exception _lastException;
        private object _window;
        private object _repositoryExplorerWidget;
        private object _ribbonWidget;
        private bool _isWidgetsInitialized;

        /// <summary>
        /// Расширение
        /// </summary>
        public IExtensionModel Extension { get; }

        /// <summary>
        /// Метаданные расширения
        /// </summary>
        public IExtensionMetadataModel Metadata => Extension.Metadata;

        /// <summary>
        /// Состояние расширения
        /// </summary>
        public ExtensionState State
        {
            get => _state;
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                }
            }
        }


        public CanExecuteResultModel LastCanExecuteResultModel
        {
            get => _lastCanExecuteResultModel ?? new CanExecuteResultModel(false, "");
            private set
            {
                if (_lastCanExecuteResultModel != value)
                {
                    _lastCanExecuteResultModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public Exception LastException
        {
            get => _lastException;
            private set
            {
                if (_lastException != value)
                {
                    _lastException = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Виджет расширения
        /// </summary>
        public object Window
        {
            get => _window;
            private set
            {
                if (_window != value)
                {
                    _window = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Виджет расширения
        /// </summary>
        public object RepositoryExplorerWidget
        {
            get => _repositoryExplorerWidget;
            private set
            {
                if (_repositoryExplorerWidget != value)
                {
                    _repositoryExplorerWidget = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Виджет расширения
        /// </summary>
        public object RibbonWidget
        {
            get => _ribbonWidget;
            private set
            {
                if (_ribbonWidget != value)
                {
                    _ribbonWidget = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Состояние инициализации виджета
        /// </summary>
        public bool IsWidgetInitialized
        {
            get => _isWidgetsInitialized;
            private set
            {
                if (_isWidgetsInitialized != value)
                {
                    _isWidgetsInitialized = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// История операций
        /// </summary>
        public ObservableCollection<OperationLog> OperationHistory { get; }

        public event EventHandler<ExtensionStateChangedEventArgs> StateChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ExtensionInstance(IExtensionModel extension)
        {
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            State = ExtensionState.Created;
            OperationHistory = new ObservableCollection<OperationLog>();

            if (extension.Metadata.AutoStart)
            {
                extension.InitializeWidgets();
                IsWidgetInitialized = true;
                InitializeWidgets();
            }
        }

        /// <summary>
        /// Запустить расширение
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            try
            {
                State = ExtensionState.Running;
                await Extension.StartAsync();
                InitializeWidgets();
                LogOperation("Start", "Расширение запущено", false);
            }
            catch (Exception ex)
            {
                State = ExtensionState.Error;
                LastException = ex;
                LogOperation("Start", $"Ошибка при запуске: {ex.Message}", true);
                throw;
            }
        }

        /// <summary>
        /// Остановить расширение
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            try
            {
                UninitializeWidget();
                await Extension.StopAsync();
                State = ExtensionState.Stopped;
                LogOperation("Stop", "Расширение остановлено", false);
            }
            catch (Exception ex)
            {
                State = ExtensionState.Error;
                LastException = ex;
                LogOperation("Stop", $"Ошибка при останове: {ex.Message}", true);
                throw;
            }
        }

        /// <summary>
        /// Выполнить основной метод
        /// </summary>
        /// <param name="element">Текущий элемент репозитория</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns></returns>
        public async Task<MainEntityBaseModel> ExecuteAsync(MainEntityBaseModel element, CancellationToken cancellationToken = default)
        {
            try
            {
                if (State != ExtensionState.Running)
                {
                    throw new InvalidOperationException("Расширение не запущено");
                }

                var result = await Extension.ExecuteAsync(element, cancellationToken);
                LogOperation("Execute", $"Выполнено для элемента: {element.Name}", false);
                return result;
            }
            catch (Exception ex)
            {
                State = ExtensionState.Error;
                LastException = ex;
                LogOperation("Execute", $"Ошибка при выполнении: {ex.Message}", true);
                throw;
            }
        }

        public async Task UpdateCanExecuteAsync(MainEntityBaseModel element)
        {
            try
            {
                LastCanExecuteResultModel = await Extension.CanExecuteAsync(element);
            }
            catch (Exception ex)
            {
                LastCanExecuteResultModel = new CanExecuteResultModel(false, $"Ошибка проверки: {ex.Message}");
            }
        }


        /// <summary>
        /// Обновить виджет обозревателя репозитория
        /// </summary>
        public void RefreshWidgets()
        {
            Window = Extension.GetMainWindow();
            RepositoryExplorerWidget = Extension.GetRepositoryExplorerWidget();
            RibbonWidget = Extension.GetRibbonWidget();
        }

        /// <summary>
        /// Инициализировать виджет обозревателя репозитория
        /// </summary>
        private void InitializeWidgets()
        {
            Extension.InitializeWidgets();

            var window = Extension.GetMainWindow();
            if (window is IExtensionWidget extWindow)
            {
                extWindow.SetExtension(Extension);
            }
            Window = window;

            var repositoryExplorerWidget = Extension.GetRepositoryExplorerWidget();
            if (repositoryExplorerWidget is IExtensionWidget extRepositoryExplorerWidget)
            {
                extRepositoryExplorerWidget.SetExtension(Extension);
            }
            RepositoryExplorerWidget = repositoryExplorerWidget;

            var ribbonWidget = Extension.GetRibbonWidget();
            if (ribbonWidget is IExtensionWidget extRibbonWidget)
            {
                extRibbonWidget.SetExtension(Extension);
            }
            RibbonWidget = ribbonWidget;
        }

        /// <summary>
        /// Деинициализировать виджет обозревателя репозитория
        /// </summary>
        private void UninitializeWidget()
        {
            try
            {
                Extension.UninitializeWidgets();

                Window = null;
                RepositoryExplorerWidget = null;
                RibbonWidget = null;

                IsWidgetInitialized = false;
            }
            catch
            {
                LogOperation("Деинициализация виджета обозревателя репозитория", "Ошибка деинициализации", isError: true);
            }
        }

        private void LogOperation(string operation, string details, bool isError)
        {
            //NotificationCriticalLevelModel criticalLevel = NotificationCriticalLevelModel.Error;
            //if (isError == false)
            //    criticalLevel = NotificationCriticalLevelModel.Info;
            //_notificationService.SendTextMessage($"{operation}. {details}", criticalLevel);
            var log = new OperationLog
            {
                Timestamp = DateTime.Now,
                Operation = operation,
                Details = details,
                IsError = isError
            };

            OperationHistory.Add(log);

            // Удерживаем только последние 50 записей
            while (OperationHistory.Count > 50)
            {
                OperationHistory.RemoveAt(0);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnStateChanged(ExtensionState oldState, ExtensionState newState)
        {
            StateChanged?.Invoke(this, new ExtensionStateChangedEventArgs(oldState, newState));
        }

    }
}
