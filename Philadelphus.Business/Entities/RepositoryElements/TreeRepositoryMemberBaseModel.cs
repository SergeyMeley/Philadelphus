using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Services;
using System.Collections.Generic;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public abstract class TreeRepositoryMemberBaseModel : MainEntityBaseModel, ITreeRepositoryMemberModel, IContentOwnerModel, IChildrenModel, ITypedModel, ISequencableModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый член репозитория"; }
        public IParentModel Parent { get; protected set; }
        public TreeRepositoryModel ParentRepository { get; protected set; }
        public long Sequence { get; set; }
        public List<ElementAttributeModel> PersonalAttributes { get; set; } = new List<ElementAttributeModel>();
        public List<ElementAttributeModel> ParentElementAttributes { get; set; } = new List<ElementAttributeModel>();
        public List<ElementAttributeValueModel> AttributeValues { get; set; } = new List<ElementAttributeValueModel>();
        public override IDataStorageModel DataStorage { get; }
        internal TreeRepositoryMemberBaseModel(Guid guid, IParentModel parent) : base(guid)
        {
            if (parent == null)
            {
                NotificationService.SendNotification($"Невозможно добавить элемент {EntityType}, выделите родительский элемент и повторите попытку!", NotificationCriticalLevelModel.Error);
                return;
            }
            Parent = parent;
            if (parent.GetType() == typeof(TreeRepositoryModel))
            {
                ParentRepository = (TreeRepositoryModel)parent;
            }
            else
            {
                ParentRepository = ((TreeRepositoryMemberBaseModel)parent).ParentRepository;
            }
        }
    }
}
