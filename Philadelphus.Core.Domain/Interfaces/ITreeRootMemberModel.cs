using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Участник корня дерева сущностей
    /// </summary>
    public interface ITreeRootMemberModel : ITreeRepositoryMemberModel
    {
        /// <summary>
        /// Родительский корень
        /// </summary>
        public TreeRootModel ParentRoot { get; }
    }
}
