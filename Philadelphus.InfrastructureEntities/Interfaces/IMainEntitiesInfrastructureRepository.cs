using Philadelphus.InfrastructureEntities.MainEntities;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface IMainEntitiesInfrastructureRepository : IInfrastructureRepository
    {
        # region [ Select ]
        public IEnumerable<TreeRoot> SelectRoots();
        public IEnumerable<TreeRoot> SelectRoots(Guid[] guids);
        public IEnumerable<TreeNode> SelectNodes();
        public IEnumerable<TreeNode> SelectNodes(Guid parentRootGuid);
        public IEnumerable<TreeLeave> SelectLeaves();
        public IEnumerable<TreeLeave> SelectLeaves(Guid parentRootGuid);
        public IEnumerable<ElementAttribute> SelectAttributes();
        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues();
        #endregion

        #region [ Insert ]
        public long InsertRoots(IEnumerable<TreeRoot> items);
        public long InsertNodes(IEnumerable<TreeNode> items);
        public long InsertLeaves(IEnumerable<TreeLeave> items);
        public long InsertAttributes(IEnumerable<ElementAttribute> items);
        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion

        #region [ Update ]
        public long UpdateRoots(IEnumerable<TreeRoot> items);
        public long UpdateNodes(IEnumerable<TreeNode> items);
        public long UpdateLeaves(IEnumerable<TreeLeave> items);
        public long UpdateAttributes(IEnumerable<ElementAttribute> items);
        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion

        #region [ Delete ]
        public long DeleteRoots(IEnumerable<TreeRoot> items);
        public long DeleteNodes(IEnumerable<TreeNode> items);
        public long DeleteLeaves(IEnumerable<TreeLeave> items);
        public long DeleteAttributes(IEnumerable<ElementAttribute> items);
        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion
    }
} 
