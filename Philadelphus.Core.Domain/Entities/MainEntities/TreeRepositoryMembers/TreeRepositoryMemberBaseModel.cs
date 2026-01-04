using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
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
        internal TreeRepositoryMemberBaseModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, dbEntity)
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
