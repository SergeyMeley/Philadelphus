using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Presentation.Services.Interfaces
{
    /// <summary>
    /// Сервис диалога выбора хранилища данных.
    /// </summary>
    public interface IDataStorageSelectionDialogService
    {
        /// <summary>
        /// Показать диалог выбора хранилища данных.
        /// </summary>
        /// <param name="dataStorages">Доступные для выбора хранилища.</param>
        /// <param name="message">Сообщение пользователю.</param>
        /// <param name="title">Заголовок диалога.</param>
        /// <returns>Выбранное хранилище или null, если выбор отменен.</returns>
        Task<IDataStorageModel?> SelectAsync(
            IEnumerable<IDataStorageModel> dataStorages,
            string message,
            string title = "Выбор хранилища");
    }
}
