using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Infrastructure.Persistence.Entities.MainEntities
{
    public class MainEntitiesCollection
    {
        public IEnumerable<TreeRoot> DbTreeRoots { get; set; }
        public IEnumerable<TreeNode> DbTreeNodes { get; set; }
        public IEnumerable<TreeLeave> DbTreeLeaves { get; set; }
        public IEnumerable<ElementAttribute> DbAttributes { get; set; }
    }
}
