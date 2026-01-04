using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITreeRootMemberModel : ITreeRepositoryMemberModel
    {
        public TreeRootModel ParentRoot { get; }
    }
}
