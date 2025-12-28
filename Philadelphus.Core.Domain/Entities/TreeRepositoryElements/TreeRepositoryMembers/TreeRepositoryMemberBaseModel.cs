using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.InfrastructureEntities.MainEntities;
using System.Collections.Generic;

namespace Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers
{
    public abstract class TreeRepositoryMemberBaseModel : MainEntityBaseModel, ITreeRepositoryMemberModel, IAttributeOwnerModel, IChildrenModel, ITypedModel, ISequencableModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый член репозитория"; }
        public IParentModel Parent { get; protected set; }
        public TreeRepositoryModel ParentRepository { get; protected set; }
        public long Sequence { get; set; }
        public List<ElementAttributeModel> Attributes 
        { get
            {
                return PersonalAttributes.Concat(ParentElementAttributes).ToList();
            }
        }
        public List<ElementAttributeModel> PersonalAttributes { get; set; } = new List<ElementAttributeModel>();
        public List<ElementAttributeModel> ParentElementAttributes { get; set; } = new List<ElementAttributeModel>();
        public override IDataStorageModel DataStorage { get; }
        internal TreeRepositoryMemberBaseModel(Guid guid, IParentModel parent, IMainEntity dbEntity) : base(guid, dbEntity)
        {
            SetParents(parent);
        }

        protected bool SetParents(IParentModel parent)
        {
            if (parent == null)
                return false;

            Parent = parent;

            if (parent is TreeRepositoryModel)
            {
                ParentRepository = (TreeRepositoryModel)parent;
                return true;
            }
            else if (parent is TreeRepositoryMemberBaseModel)
            {
                ParentRepository = ((TreeRepositoryMemberBaseModel)parent).ParentRepository;
                return true;
            }

            return false;
        }
    }
}
