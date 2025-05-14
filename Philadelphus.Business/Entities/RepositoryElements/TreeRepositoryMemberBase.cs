using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using System.Collections.Generic;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class TreeRepositoryMemberBase : MainEntityBase, ITreeRepositoryMember, IContentOwner, IChildren, ITyped, ISequencable
    {
        public IParent Parent { get; protected set; }
        public TreeRepository ParentRepository { get; protected set; }
        public long Sequence { get; set; }
        public IEnumerable<ElementAttribute> PersonalAttributes { get; set; } = new List<ElementAttribute>();
        public IEnumerable<ElementAttribute> ParentElementAttributes { get; set; } = new List<ElementAttribute>();
        public IEnumerable<ElementAttributeValue> AttributeValues { get; set; } = new List<ElementAttributeValue>();

        public TreeRepositoryMemberBase(Guid guid, IParent parent) : base(guid)
        {
            Parent = parent;
            if (parent.GetType() == typeof(TreeRepository))
            {
                ParentRepository = (TreeRepository)parent;
            }
            else
            {
                ParentRepository = ((TreeRepositoryMemberBase)parent).ParentRepository;
            }
        }
    }
}
