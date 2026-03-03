using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Содержимое
    /// </summary>
    public interface IContentModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Владелец
        /// </summary>
        public IOwnerModel Owner { get; }

        /// <summary>
        /// Все владельцы (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive { get; }

        /// <summary>
        /// Сменить владельца
        /// </summary>
        public bool ChangeOwner(IOwnerModel newOwner);
    }
}
