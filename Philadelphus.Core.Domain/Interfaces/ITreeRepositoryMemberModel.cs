using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Interfaces
{
    /// <summary>
    /// Участник репозитория
    /// </summary>
    public interface ITreeRepositoryMemberModel : IMainEntityModel
    {
        /// <summary>
        /// Родительский репозиторий
        /// </summary>
        public TreeRepositoryModel ParentRepository { get; }
    }
}
