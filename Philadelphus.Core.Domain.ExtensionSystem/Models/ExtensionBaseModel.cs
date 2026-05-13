using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Базовый класс для расширений
    /// </summary>
    public abstract class ExtensionBaseModel : IExtensionModel
    {
        /// <summary>
        /// Метаданные.
        /// </summary>
        public virtual IExtensionMetadataModel Metadata { get; protected set; }

        /// <summary>
        /// Запуск расширения (асинхронно).
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public virtual Task StartAsync() => Task.CompletedTask;

        /// <summary>
        /// Остановка расширения (асинхронно).
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public virtual Task StopAsync() => Task.CompletedTask;

        /// <summary>
        /// Асинхронно проверяет возможность выполнения операции.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public abstract Task<CanExecuteResultModel> CanExecuteAsync(
            IMainEntityModel element);

        /// <summary>
        /// Выполнить основной метод расширения (асинхронно).
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public abstract Task<IMainEntityModel> ExecuteAsync(
            IMainEntityModel element,
            IPhiladelphusRepositoryService service,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Получает виджет панели расширения формы работы с репозиторием.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        
        public virtual object GetRepositoryExplorerWidget() => null;
        
        /// <summary>
        /// Получает виджер панели инструментов.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        public virtual object GetRibbonWidget() => null;

        /// <summary>
        /// Получает основное окно расширения.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        public virtual object GetMainWindow() => null;

        /// <summary>
        /// Инициализирует виджеты.
        /// </summary>
        public virtual void InitializeWidgets() { }

        /// <summary>
        /// Деинициализирует виджеты.
        /// </summary>
        public virtual void UninitializeWidgets() { }
    }

}
