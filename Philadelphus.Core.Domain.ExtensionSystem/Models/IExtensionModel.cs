using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Основной интерфейс расширения
    /// </summary>
    public interface IExtensionModel
    {
        /// <summary>
        /// Метаданные расширения
        /// </summary>
        IExtensionMetadataModel Metadata { get; }

        /// <summary>
        /// Запустить расширение (инициализация)
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Остановить расширение (очистка ресурсов)
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Проверить возможность выполнения основного метода
        /// </summary>
        Task<CanExecuteResultModel> CanExecuteAsync(TreeRepositoryMemberBaseModel element);

        /// <summary>
        /// Основной метод расширения
        /// Принимает выбранный элемент и может возвращать измененный элемент
        /// </summary>
        Task<TreeRepositoryMemberBaseModel> ExecuteAsync(TreeRepositoryMemberBaseModel element, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить виджет расширения (опционально)
        /// </summary>
        object GetWidget();

        /// <summary>
        /// Получить основное окно расширения (опционально)
        /// </summary>
        object GetMainWindow();

        /// <summary>
        /// Инициализировать виджет при запуске расширения
        /// </summary>
        void InitializeWidget();

        /// <summary>
        /// Деинициализировать виджет при останове расширения
        /// </summary>
        void UninitializeWidget();
    }
}
