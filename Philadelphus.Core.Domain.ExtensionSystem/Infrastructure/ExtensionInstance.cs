using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        private object _widget;
        private bool _isWidgetInitialized;

        public IExtensionModel Extension { get; }
        public IExtensionMetadataModel Metadata => Extension.Metadata;

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

        public object Widget
        {
            get => _widget;
            private set
            {
                if (_widget != value)
                {
                    _widget = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWidgetInitialized
        {
            get => _isWidgetInitialized;
            private set
            {
                if (_isWidgetInitialized != value)
                {
                    _isWidgetInitialized = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<OperationLog> OperationHistory { get; }

        public event EventHandler<ExtensionStateChangedEventArgs> StateChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ExtensionInstance(IExtensionModel extension)
        {
            Extension = extension ?? throw new ArgumentNullException(nameof(extension));
            State = ExtensionState.Created;
            OperationHistory = new ObservableCollection<OperationLog>();
        }

        public async Task StartAsync()
        {
            try
            {
                State = ExtensionState.Running;
                await Extension.StartAsync();
                InitializeWidget();
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

        public async Task<TreeRepositoryMemberBaseModel> ExecuteAsync(TreeRepositoryMemberBaseModel element, CancellationToken cancellationToken = default)
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

        public async Task UpdateCanExecuteAsync(TreeRepositoryMemberBaseModel element)
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

        public void RefreshWidget()
        {
            Widget = Extension.GetWidget();
        }

        private void InitializeWidget()
        {
            try
            {
                Extension.InitializeWidget();
                Widget = Extension.GetWidget();
                IsWidgetInitialized = true;
            }
            catch
            {
                IsWidgetInitialized = false;
            }
        }

        private void UninitializeWidget()
        {
            try
            {
                Extension.UninitializeWidget();
                Widget = null;
                IsWidgetInitialized = false;
            }
            catch
            {
                // Игнорируем ошибки при деинициализации
            }
        }

        private void LogOperation(string operation, string details, bool isError)
        {
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
