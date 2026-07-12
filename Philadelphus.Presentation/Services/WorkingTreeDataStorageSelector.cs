using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Services
{
    /// <summary>
    /// Выбирает хранилище для нового рабочего дерева с учётом настройки репозитория.
    /// </summary>
    internal static class WorkingTreeDataStorageSelector
    {
        private const string SelectionMessage =
            "Хранилище участников кустарника по умолчанию не задано. Выберите хранилище для нового рабочего дерева.";

        /// <summary>
        /// Возвращает допустимое хранилище по умолчанию или запрашивает явный выбор пользователя.
        /// </summary>
        /// <param name="defaultDataStorage">Хранилище участников кустарника по умолчанию.</param>
        /// <param name="repositoryDataStorages">Хранилища, доступные репозиторию.</param>
        /// <param name="selectionDialogService">Сервис диалога выбора хранилища.</param>
        /// <returns>Выбранное хранилище или <see langword="null" />, если пользователь отменил выбор.</returns>
        internal static Task<IDataStorageModel?> SelectAsync(
            IDataStorageModel? defaultDataStorage,
            IEnumerable<IDataStorageModel> repositoryDataStorages,
            IDataStorageSelectionDialogService selectionDialogService)
        {
            ArgumentNullException.ThrowIfNull(repositoryDataStorages);
            ArgumentNullException.ThrowIfNull(selectionDialogService);

            var availableDataStorages = repositoryDataStorages.ToList();
            if (defaultDataStorage?.HasShrubMembersInfrastructureRepository == true
                && availableDataStorages.Any(x => x.Uuid == defaultDataStorage.Uuid))
            {
                return Task.FromResult<IDataStorageModel?>(defaultDataStorage);
            }

            return selectionDialogService.SelectAsync(
                availableDataStorages.Where(x => x.HasShrubMembersInfrastructureRepository),
                SelectionMessage);
        }
    }
}
