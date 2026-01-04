using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers;

namespace Philadelphus.Core.Domain.Interfaces
{
    public interface ITreeRepositoryMemberModel : IMainEntityModel
    {
        public TreeRepositoryModel ParentRepository { get; }
    }
}
