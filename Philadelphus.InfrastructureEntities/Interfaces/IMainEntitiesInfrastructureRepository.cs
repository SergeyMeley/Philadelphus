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
        public long InsertRoots(IEnumerable<TreeRoot> roots);
        public long InsertNodes(IEnumerable<TreeNode> nodes);
        public long InsertLeaves(IEnumerable<TreeLeave> leaves);
        public long InsertAttributes(IEnumerable<ElementAttribute> attributes);
        public long InsertAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries);
        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues);
        #endregion
        #region [ Delete ]
        public long DeleteRoots(IEnumerable<TreeRoot> roots);
        public long DeleteNodes(IEnumerable<TreeNode> nodes);
        public long DeleteLeaves(IEnumerable<TreeLeave> leaves);
        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes);
        public long DeleteAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries);
        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues);
        #endregion
        #region [ Update ]
        public long UpdateRoots(IEnumerable<TreeRoot> roots);
        public long UpdateNodes(IEnumerable<TreeNode> nodes);
        public long UpdateLeaves(IEnumerable<TreeLeave> leaves);
        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes);
        public long UpdateAttributeEntries(IEnumerable<TreeElementAttribute> attributeEntries);
        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> attributeValues);
        #endregion
    }
} 
