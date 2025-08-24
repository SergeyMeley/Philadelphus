using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Philadelphus.Business.Helpers;
using System.Collections.ObjectModel;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.Business.Entities.OtherEntities;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeRootModel : TreeRepositoryMemberBaseModel, IHavingOwnStorageModel, IChildrenModel, IParentModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Root; }
        public InfrastructureTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public EntityElementTypeModel ElementType { get; set; }
        public IParentModel Parent {  get; private set; }
        public IEnumerable<IChildrenModel> Childs { get; set; }
        internal TreeRootModel(Guid guid, IParentModel parent) : base(guid, parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                return;
            }
            if (parent.GetType() == typeof(TreeRepositoryModel))
            {
                Guid = guid;
                Parent = parent;
                ParentRepository = (TreeRepositoryModel)parent;
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new NotificationModel("Корень может быть добавлен только в репозиторий!", NotificationCriticalLevelModel.Error));
            }
        }
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in ParentRepository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, "Новый корень");
            Childs = new ObservableCollection<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this);
        }
    }
}
