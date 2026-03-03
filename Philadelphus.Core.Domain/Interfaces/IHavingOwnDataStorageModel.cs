using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Владелец собственного хранилища. 
    /// Реализует возможность наличия независимой инфраструктуры (места и способа хранения данных)
    /// </summary>
    public interface IHavingOwnDataStorageModel
    {
        /// <summary>
        /// Собственное хранилище
        /// </summary>
        public IDataStorageModel OwnDataStorage { get; }

        /// <summary>
        /// Изменить собственное хранилище
        /// </summary>
        /// <param name="storage"></param>
        /// <returns></returns>
        public bool ChangeDataStorage(IDataStorageModel storage);
    }
}
