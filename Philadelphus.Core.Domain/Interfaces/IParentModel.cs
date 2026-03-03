using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Родитель
    /// </summary>
    public interface IParentModel : ILinkableByUuidModel
    {
        /// <summary>
        /// Наследники
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> Childs { get; }

        /// <summary>
        /// Все наследники (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IChildrenModel> AllChildsRecursive { get; }
    }
}
