using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class RepositoryElementBase : MainEntityBase, IHavingParent, ITyped, ISequencable
    {
        public IHavingChilds Parent { get; protected set; }
        public TreeRepository ParentRepository { get; protected set; }
        public TreeRoot ParentRoot { get; protected set; }
        public long Sequence { get; set; }
        public RepositoryElementBase(Guid guid, IHavingChilds parent) : base(guid)
        {
            Parent = parent;
            if (parent.GetType() == typeof(TreeRepository))
            {
                ParentRepository = (TreeRepository)parent;
            }
            else
            {
                ParentRepository = ((RepositoryElementBase)parent).ParentRepository;
            }
        }
    }
}
