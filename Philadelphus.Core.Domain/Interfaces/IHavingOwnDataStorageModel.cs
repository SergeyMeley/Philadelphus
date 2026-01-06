using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Владелец собственного хранилища. 
    /// Реализует возможность наличия независимой инфраструктуры (места и способа хранения данных)
    /// </summary>
    internal interface IHavingOwnDataStorageModel
    {
        /// <summary>
        /// Собственное хранилище
        /// </summary>
        internal IDataStorageModel OwnDataStorage { get; }

        /// <summary>
        /// Изменить собственное хранилище
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public bool ChangeDataStorage(IDataStorageModel storage);
    }
}
