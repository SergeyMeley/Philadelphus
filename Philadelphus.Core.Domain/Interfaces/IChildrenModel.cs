using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Наследник
    /// </summary>
    public interface IChildrenModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Родитель
        /// </summary>
        public IParentModel Parent { get; }

        /// <summary>
        /// Все родители (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IOwnerModel> AllParentsRecursive { get; }

        /// <summary>
        /// Сменить родителя
        /// </summary>
        public bool ChangeParent(IParentModel newParent);
    }
}
