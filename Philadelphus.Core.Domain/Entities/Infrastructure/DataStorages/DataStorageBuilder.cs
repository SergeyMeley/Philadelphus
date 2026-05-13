using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;

namespace Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages
{
    /// <summary>
    /// Строитель хранилища данных
    /// </summary>
    public class DataStorageBuilder
    {
        /// <summary>
        /// Хранилище данных (доменная модель)
        /// </summary>
        private IDataStorageModel _storageModel;

        /// <summary>
        /// Строитель хранилища данных
        /// </summary>
        public DataStorageBuilder()
        {
        }

        /// <summary>
        /// Задать основные параметры хранилища данных
        /// </summary>
        /// <param name="name">Наименование хранилища данных</param>
        /// <param name="description">Описание хранилища данных</param>
        /// <param name="uuid">Уникальный идентификатор хранилища данных</param>
        /// <param name="infrastructureType">Тип хранилища данных</param>
        /// <param name="isDisabled">Состояние отключенности хранилища данных</param>
        /// <returns>Строитель хранилища данных</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если наименование пусто или некорректно</exception>
        public DataStorageBuilder SetGeneralParameters(ILogger logger, string name, string description, Guid uuid, InfrastructureTypes infrastructureType, bool isDisabled)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentOutOfRangeException.ThrowIfEqual(uuid, Guid.Empty);

            _storageModel = new DataStorageModel(logger, uuid, name, description, infrastructureType, isDisabled);
            return this;
        }

        /// <summary>
        /// Добавить репозиторий БД
        /// </summary>
        /// <param name="repository">Репозиторий БД</param>
        /// <returns>Строитель хранилища данных</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public DataStorageBuilder SetRepository(IInfrastructureRepository repository)
        {
            if (repository == null)
                return this;
            if (_storageModel == null)
                throw new InvalidOperationException("Сначала необходимо назначить основные параметры");
            if (_storageModel.IsHidden)
                return this;

            if (_storageModel.InfrastructureRepositories.ContainsKey(repository.EntityGroup))
            {
                _storageModel.InfrastructureRepositories[repository.EntityGroup] = repository;
            }
            else
            {
                _storageModel.InfrastructureRepositories.Add(repository.EntityGroup, repository);
            }
            return this;
        }

        /// <summary>
        /// Получить хранилище данных
        /// </summary>
        /// <returns>Готовое хранилище данных. Возвращает null, если валидация не прошла.</returns>
        public IDataStorageModel Build()
        {
            if (_storageModel == null)
                return null;

            if (string.IsNullOrEmpty(_storageModel.Name))
            {
                return null;
            }

            if (_storageModel.InfrastructureRepositories == null || _storageModel.InfrastructureRepositories.Count == 0)
                return null;

            _storageModel.CheckAvailableAsync();
            return _storageModel;
        }
    }
}
