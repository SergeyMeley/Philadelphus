using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IPhiladelphusRepositoriesMembersInfrastructureRepository : IInfrastructureRepository
    {
        # region [ Select ]

        public IEnumerable<WorkingTree> SelectTrees();
        public IEnumerable<TreeRoot> SelectRoots();
        public IEnumerable<WorkingTree> SelectTrees(Guid[] uuids);
        public IEnumerable<TreeRoot> SelectRoots(Guid[] owningTreesUuids);
        public IEnumerable<TreeNode> SelectNodes();
        public IEnumerable<TreeNode> SelectNodes(Guid[] owningTreesUuids);
        public IEnumerable<TreeLeave> SelectLeaves();
        public IEnumerable<TreeLeave> SelectLeaves(Guid[] owningTreesUuids);
        public IEnumerable<ElementAttribute> SelectAttributes();

        #endregion

        #region [ Insert ]

        public long InsertTrees(IEnumerable<WorkingTree> items);
        public long InsertRoots(IEnumerable<TreeRoot> items);
        public long InsertNodes(IEnumerable<TreeNode> items);
        public long InsertLeaves(IEnumerable<TreeLeave> items);
        public long InsertAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Update ]

        public long UpdateTrees(IEnumerable<WorkingTree> items);
        public long UpdateRoots(IEnumerable<TreeRoot> items);
        public long UpdateNodes(IEnumerable<TreeNode> items);
        public long UpdateLeaves(IEnumerable<TreeLeave> items);
        public long UpdateAttributes(IEnumerable<ElementAttribute> items);

        #endregion

        #region [ Delete ]

        public long SoftDeleteTrees(IEnumerable<WorkingTree> items);
        public long SoftDeleteRoots(IEnumerable<TreeRoot> items);
        public long SoftDeleteNodes(IEnumerable<TreeNode> items);
        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items);
        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items);

        #endregion
    }
} 
