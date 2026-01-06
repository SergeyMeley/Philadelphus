using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

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
        /// <exception cref="ArgumentException"></exception>
        public DataStorageBuilder SetGeneralParameters(string name, string description, Guid uuid, InfrastructureTypes infrastructureType, bool isDisabled)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) /*|| uuid == Guid.Empty*/)  //TODO: Исправить костыль
                throw new ArgumentException("Переданы некорректные параметры");
            _storageModel = new DataStorageModel(uuid, name, description, infrastructureType, isDisabled);
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
            if (_storageModel.IsDisabled)
                return this;
            if (_storageModel == null)
                throw new ArgumentNullException("Сначала необходимо назначить основные параметры");
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
        /// <returns>Готовое хранилище данных</returns>
        public IDataStorageModel Build()
        {
            if (_storageModel == null)
                return null;
            if (string.IsNullOrEmpty(_storageModel.Name) 
                || string.IsNullOrEmpty(_storageModel.Description)
                /*|| _storageModel.Uuid == Guid.Empty*/)    //TODO: Исправить костыль
                return null;
            if (_storageModel.InfrastructureRepositories == null || _storageModel.InfrastructureRepositories.Count == 0)
                return null;
            return _storageModel;
        }
    }
}
