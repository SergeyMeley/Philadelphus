using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
