using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class RepositoryElementBase : MainEntityBase, IHavingParent, ITyped, ISequencable
    {
        public IHavingChilds Parent { get; private set; }
        internal TreeRepository Repository { get; private set; }
        public long Sequence { get; set; }
        public RepositoryElementBase(Guid guid, IHavingChilds parent) : base(guid)
        {
            Parent = parent;
            if (parent.GetType() == typeof(TreeRepository))
            {
                Repository = (TreeRepository)parent;
            }
            else
            {
                Repository = ((RepositoryElementBase)parent).Repository;
            }
        }
    }
}
