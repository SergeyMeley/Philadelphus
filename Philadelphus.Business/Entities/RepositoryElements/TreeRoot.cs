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
    public class TreeRoot : TreeRepositoryMemberBase, IHavingOwnStorage, IChildren, IParent
    {
        public override EntityTypes EntityType { get => EntityTypes.Root; }
        public InfrastructureTypes InfrastructureRepositoryType { get; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public EntityElementType ElementType { get; set; }
        public IParent Parent {  get; private set; }
        public IEnumerable<IChildren> Childs { get; set; }
        internal TreeRoot(Guid guid, IParent parent) : base(guid, parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                return;
            }
            if (parent.GetType() == typeof(TreeRepository))
            {
                Guid = guid;
                Parent = parent;
                ParentRepository = (TreeRepository)parent;
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new Notification("Корень может быть добавлен только в репозиторий!", NotificationCriticalLevel.Error));
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
            Childs = new ObservableCollection<IChildren>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
