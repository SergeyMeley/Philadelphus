using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Core.Domain.Services.Interfaces
{
    /// <summary>
    /// Сервис работы с хранилищами данных
    /// </summary>
    public interface IDataStoragesService
    {
        #region [ Props ]

        #endregion

        #region [ Get + Load ]

        /// <summary>
        /// Получить коллекцию хранилищ данных
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="getInfrastructureRepository">Функция получения инфраструктурного репозитория.</param>
        public IEnumerable<IDataStorageModel> GetStoragesModels(
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository);

        #endregion

        #region [ Save ]

        #endregion

        #region [ Create + Add ]

        /// <summary>
        /// Создать основное хранилище данных
        /// </summary>
        /// <param name="storagesConfigFullPath">Путь к настроечному файлу хранилищ данных</param>
        /// <param name="repositoryHeadersConfigFullPath">Путь к настроечному файлу запусков репозиториев</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="basePath">Базовый путь.</param>
        public IDataStorageModel CreateMainDataStorageModel(DirectoryInfo basePath);

        /// <summary>
        /// /// <summary>
        /// Создать хранилище данных
        /// <param name="name">Наименование</param>
        /// <param name="desctiption">Описание</param>
        /// <param name="connectionString">Строка подключенияя</param>
        /// <param name="getInfrastructureRepository">Функция получения инфраструктурного репозитория.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public IDataStorageModel CreateDataStorageModel(string name, string desctiption, ConnectionStringsContainer connectionString,
            Func<ConnectionStringsContainer, InfrastructureTypes, InfrastructureEntityGroups, IInfrastructureRepository> getInfrastructureRepository);

        #endregion

        #region [ Modify ]

        #endregion

        #region [ Delete + Remove ]

        #endregion

    }
}
