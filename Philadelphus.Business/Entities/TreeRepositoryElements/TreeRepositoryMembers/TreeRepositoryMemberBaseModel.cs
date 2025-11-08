using Philadelphus.Business.Entities.ElementsContent;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System.Collections.Generic;

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers
{
    public abstract class TreeRepositoryMemberBaseModel : MainEntityBaseModel, ITreeRepositoryMemberModel, IAttributeOwnerModel, IChildrenModel, ITypedModel, ISequencableModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый член репозитория"; }
        public IParentModel Parent { get; protected set; }
        public TreeRepositoryModel ParentRepository { get; protected set; }
        public long Sequence { get; set; }
        public List<ElementAttributeModel> PersonalAttributes { get; set; } = new List<ElementAttributeModel>();
        public List<ElementAttributeModel> ParentElementAttributes { get; set; } = new List<ElementAttributeModel>();
        public List<ElementAttributeValueModel> AttributeValues { get; set; } = new List<ElementAttributeValueModel>();
        public override IDataStorageModel DataStorage { get; }
        internal TreeRepositoryMemberBaseModel(Guid guid, IParentModel parent, IMainEntity dbEntity) : base(guid, dbEntity)
        {
            SetParents(parent);
        }

        protected bool SetParents(IParentModel parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification($"Невозможно добавить элемент {EntityType}, выделите родительский элемент и повторите попытку!", NotificationCriticalLevelModel.Error);
                return false;
            }

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

            NotificationService.SendNotification($"Ошибка присвоения родительского репозитория", NotificationCriticalLevelModel.Error);
            return false;
        }
    }
}
