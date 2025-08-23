using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent
{
    public class ElementAttributeModel : MainEntityBaseModel, ITreeElementContentModek, ITreeRootMemberModel, ITreeRepositoryMemberModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public ValueTypesModel ValueType { get; set; }
        public IEnumerable<ElementAttributeValueModel>? ValueList { get; set; }
        public ElementAttributeValueModel? AttributeValue { get; set; }
        public IContentOwnerModel Owner { get; set; }
        public TreeRootModel ParentRoot { get; set; }
        public TreeRepositoryModel ParentRepository { get; set; }
        public ElementAttributeModel(Guid guid, IContentOwnerModel owner) : base(guid)
        {
            Guid = guid;
            Owner = owner;
            if (owner.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)))
            {
                ParentRepository = ((ITreeRepositoryMemberModel)owner).ParentRepository;
                if (owner.GetType().IsAssignableTo(typeof(ITreeRootMemberModel)))
                {
                    ParentRoot = ((ITreeRootMemberModel)owner).ParentRoot;
                }
                else
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new NotificationModel("Атрибут может быть добавлен только участнику репозитория!", NotificationCriticalLeveModel.Error));
            }
        }


        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in ParentRepository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            Name = NamingHelper.GetNewName(existNames, "Новый атрибут");
        }
    }
}
