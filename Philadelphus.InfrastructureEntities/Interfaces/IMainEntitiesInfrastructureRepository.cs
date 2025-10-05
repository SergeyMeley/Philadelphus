using Philadelphus.InfrastructureEntities.MainEntities;

namespace Philadelphus.InfrastructureEntities.Interfaces
{
    public interface IMainEntitiesInfrastructureRepository : IInfrastructureRepository
    {
        # region [ Select ]
        public IEnumerable<TreeRoot> SelectRoots();
        public IEnumerable<TreeNode> SelectNodes();
        public IEnumerable<TreeLeave> SelectLeaves();
        public IEnumerable<ElementAttribute> SelectAttributes();
        public IEnumerable<TreeElementAttribute> SelectAttributeEntries();
        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues();
        #endregion
        #region [ Insert ]
        public long InsertRoots(IEnumerable<TreeRoot> items);
        public long InsertNodes(IEnumerable<TreeNode> items);
        public long InsertLeaves(IEnumerable<TreeLeave> items);
        public long InsertAttributes(IEnumerable<ElementAttribute> items);
        public long InsertAttributeEntries(IEnumerable<TreeElementAttribute> items);
        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion
        #region [ Delete ]
        public long DeleteRoots(IEnumerable<TreeRoot> items);
        public long DeleteNodes(IEnumerable<TreeNode> items);
        public long DeleteLeaves(IEnumerable<TreeLeave> items);
        public long DeleteAttributes(IEnumerable<ElementAttribute> items);
        public long DeleteAttributeEntries(IEnumerable<TreeElementAttribute> items);
        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion
        #region [ Update ]
        public long UpdateRoots(IEnumerable<TreeRoot> items);
        public long UpdateNodes(IEnumerable<TreeNode> items);
        public long UpdateLeaves(IEnumerable<TreeLeave> items);
        public long UpdateAttributes(IEnumerable<ElementAttribute> items);
        public long UpdateAttributeEntries(IEnumerable<TreeElementAttribute> items);
        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> items);
        #endregion
    }
} 
