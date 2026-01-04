using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Реализует возможность наличия независимой инфраструктуры (места и способа хранения данных)
    /// </summary>
    internal interface IHavingOwnDataStorageModel
    {
        internal IDataStorageModel OwnDataStorage { get; }
        public bool ChangeDataStorage(IDataStorageModel storage);
    }
}
