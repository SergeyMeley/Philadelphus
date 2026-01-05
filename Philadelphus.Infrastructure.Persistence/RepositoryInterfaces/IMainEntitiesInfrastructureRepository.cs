using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IMainEntitiesInfrastructureRepository : IInfrastructureRepository
    {
        # region [ Select ]

        public IEnumerable<TreeRoot> SelectRoots();
        public IEnumerable<TreeRoot> SelectRoots(Guid[] uuids);
        public IEnumerable<TreeNode> SelectNodes();
        public IEnumerable<TreeNode> SelectNodes(Guid[] parentRootUuids);
        public IEnumerable<TreeLeave> SelectLeaves();
        public IEnumerable<TreeLeave> SelectLeaves(Guid[] parentRootUuids);
        public IEnumerable<ElementAttribute> SelectAttributes();

        #endregion

        #region [ Insert ]

        public long InsertRoots(IEnumerable<TreeRoot> items);
        public long InsertNodes(IEnumerable<TreeNode> items);
        public long InsertLeaves(IEnumerable<TreeLeave> items);
        public long InsertAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Update ]

        public long UpdateRoots(IEnumerable<TreeRoot> items);
        public long UpdateNodes(IEnumerable<TreeNode> items);
        public long UpdateLeaves(IEnumerable<TreeLeave> items);
        public long UpdateAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Delete ]

        public long DeleteRoots(IEnumerable<TreeRoot> items);
        public long DeleteNodes(IEnumerable<TreeNode> items);
        public long DeleteLeaves(IEnumerable<TreeLeave> items);
        public long DeleteAttributes(IEnumerable<ElementAttribute> items);

        #endregion
    }
} 
