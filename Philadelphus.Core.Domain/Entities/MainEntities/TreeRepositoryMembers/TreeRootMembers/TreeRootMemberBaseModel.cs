using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
{
    public abstract class TreeRootMemberBaseModel : TreeRepositoryMemberBaseModel, ITreeRootMemberModel
    {
        public TreeRootModel ParentRoot { get; protected set; }
        public override EntityTypesModel EntityType => throw new NotImplementedException();
        public TreeRootMemberBaseModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
        {
            SetParents(parent);
        }

        protected bool SetParents(IParentModel parent)
        {
            if (parent == null)
                return false;
            if (base.SetParents(parent) == false)
                return false;
            if (parent is TreeRootModel)
            {
                ParentRoot = (TreeRootModel)parent;
                return true;
            }
            else if (parent is TreeRootMemberBaseModel)
            {
                ParentRoot = ((TreeRootMemberBaseModel)parent).ParentRoot;
                return true;
            }
            return false;
        }
    }
}
