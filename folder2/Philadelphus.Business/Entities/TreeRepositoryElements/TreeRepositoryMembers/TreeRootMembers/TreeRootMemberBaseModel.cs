using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers
{
    public abstract class TreeRootMemberBaseModel : TreeRepositoryMemberBaseModel, ITreeRootMemberModel
    {
        public TreeRootModel ParentRoot { get; protected set; }
        public override EntityTypesModel EntityType => throw new NotImplementedException();
        public TreeRootMemberBaseModel(Guid guid, IParentModel parent, IMainEntity dbEntity) : base(guid, parent, dbEntity)
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
