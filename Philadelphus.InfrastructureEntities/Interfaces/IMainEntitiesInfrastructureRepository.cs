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
        public IEnumerable<AttributeEntry> SelectAttributeEntries();
        public IEnumerable<AttributeValue> SelectAttributeValues();
        #endregion
        #region [ Insert ]
        public long InsertRoots(IEnumerable<TreeRoot> roots);
        public long InsertNodes(IEnumerable<TreeNode> nodes);
        public long InsertLeaves(IEnumerable<TreeLeave> leaves);
        public long InsertAttributes(IEnumerable<ElementAttribute> attributes);
        public long InsertAttributeEntries(IEnumerable<AttributeEntry> attributeEntries);
        public long InsertAttributeValues(IEnumerable<AttributeValue> attributeValues);
        #endregion
        #region [ Delete ]
        public long DeleteRoots(IEnumerable<TreeRoot> roots);
        public long DeleteNodes(IEnumerable<TreeNode> nodes);
        public long DeleteLeaves(IEnumerable<TreeLeave> leaves);
        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes);
        public long DeleteAttributeEntries(IEnumerable<AttributeEntry> attributeEntries);
        public long DeleteAttributeValues(IEnumerable<AttributeValue> attributeValues);
        #endregion
        #region [ Update ]
        public long UpdateRoots(IEnumerable<TreeRoot> roots);
        public long UpdateNodes(IEnumerable<TreeNode> nodes);
        public long UpdateLeaves(IEnumerable<TreeLeave> leaves);
        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes);
        public long UpdateAttributeEntries(IEnumerable<AttributeEntry> attributeEntries);
        public long UpdateAttributeValues(IEnumerable<AttributeValue> attributeValues);
        #endregion
    }
} 
