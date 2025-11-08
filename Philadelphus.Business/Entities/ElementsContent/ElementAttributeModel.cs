using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.ElementsContent
{
    public class ElementAttributeModel : MainEntityBaseModel, ITreeElementContentModel, ITreeRootMemberModel, ITreeRepositoryMemberModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public ValueTypesModel ValueType { get; set; }
        public IEnumerable<ElementAttributeValueModel>? ValueList { get; set; }
        public ElementAttributeValueModel? AttributeValue { get; set; }
        public IAttributeOwnerModel Owner { get; set; }
        public TreeRootModel ParentRoot { get; set; }
        public TreeRepositoryModel ParentRepository { get; set; }

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public ElementAttributeModel(Guid guid, IAttributeOwnerModel owner, IMainEntity dbEntity) : base(guid, dbEntity)
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
                NotificationService.Notifications.Add(new NotificationModel("Атрибут может быть добавлен только участнику репозитория!", NotificationCriticalLevelModel.Error));
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
