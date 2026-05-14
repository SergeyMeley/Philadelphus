using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs
{
    /// <summary>
    /// Модель представления для экземпляра расширения.
    /// </summary>
    public class ExtensionInstanceVM : ViewModelBase
    {
        private readonly ExtensionInstance _extensionInstance;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExtensionInstanceVM" />.
        /// </summary>
        /// <param name="extensionInstance">Экземпляр расширения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ExtensionInstanceVM(ExtensionInstance extensionInstance)
        {
            ArgumentNullException.ThrowIfNull(extensionInstance);

            _extensionInstance = extensionInstance;
            _extensionInstance.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name => _extensionInstance.Metadata.Name;
       
        /// <summary>
        /// Описание.
        /// </summary>
        public string Description => _extensionInstance.Metadata.Description;
       
        /// <summary>
        /// Версия.
        /// </summary>
        public string Version => _extensionInstance.Metadata.Version;

       
        /// <summary>
        /// Состояние.
        /// </summary>
        public ExtensionState State => _extensionInstance.State;
      
        /// <summary>
        /// Признак возможности выполнения операции.
        /// </summary>
        public bool CanExecute => _extensionInstance.LastCanExecuteResultModel.CanExecute;
       
        /// <summary>
        /// Указывает состояние или доступность сообщения.
        /// </summary>
        public string CanExecuteMessage => _extensionInstance.LastCanExecuteResultModel.Message;
      
        /// <summary>
        /// Окно.
        /// </summary>
        public object Window => _extensionInstance.Window;
       
        /// <summary>
        /// Виджет ленты.
        /// </summary>
        public object RibbonWidget => _extensionInstance.RibbonWidget;
       
        /// <summary>
        /// Репозиторий.
        /// </summary>
        public object RepositoryExplorerWidget => _extensionInstance.RepositoryExplorerWidget;
       
        /// <summary>
        /// Признак инициализации виджетов.
        /// </summary>
        public bool IsWidgetsInitialized => _extensionInstance.IsWidgetInitialized;

        /// <summary>
        /// История операций.
        /// </summary>
        public ObservableCollection<OperationLog> OperationHistory => _extensionInstance.OperationHistory;

        /// <summary>
        /// Выполняет операцию StartAsync.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task StartAsync()
        {
            await _extensionInstance.StartAsync();
        }

        /// <summary>
        /// Выполняет операцию StopAsync.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public async Task StopAsync()
        {
            await _extensionInstance.StopAsync();
        }

        /// <summary>
        /// Выполняет операцию ExecuteAsync.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public async Task ExecuteAsync(IMainEntityModel element, IPhiladelphusRepositoryService service)
        {
            ArgumentNullException.ThrowIfNull(element);
            ArgumentNullException.ThrowIfNull(service);

            await _extensionInstance.ExecuteAsync(element, service);
        }

        /// <summary>
        /// Обновляет данные UpdateCanExecuteAsync.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public async Task UpdateCanExecuteAsync(IMainEntityModel element)
        {
            ArgumentNullException.ThrowIfNull(element);

            await _extensionInstance.UpdateCanExecuteAsync(element);
        }

    }
}
