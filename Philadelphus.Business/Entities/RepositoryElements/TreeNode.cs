using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeNode : TreeRepositoryMemberBase, IParent, ITreeRootMember
    {
        public override EntityTypes EntityType { get => EntityTypes.Node; }
        public IMainEntitiesInfrastructure Infrastructure { get; private set; }
        public EntityElementType ElementType { get; set; }
        public IEnumerable<IChildren> Childs { get; set; }
        public TreeRoot ParentRoot { get; private set; }
        public TreeNode(Guid guid, IParent parent) : base(guid, parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                return;
            }
            Guid = guid;
            Parent = parent;
            if (parent.GetType().IsAssignableTo(typeof(ITreeRepositoryMember)))
            {
                if (parent.GetType() == typeof(TreeRoot))
                {
                    ParentRoot = (TreeRoot)parent;

                }
                else if (parent.GetType().IsAssignableTo(typeof(ITreeRootMember)))
                {
                    ParentRoot = ((ITreeRootMember)parent).ParentRoot;
                    ParentRepository = ((ITreeRootMember)parent).ParentRepository;
                }
                else
                {
                    NotificationService.Notifications.Add(new Notification("Узел может быть добавлен только в другой узел или корень!", NotificationCriticalLevel.Error));
                }
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new Notification("Узел может быть добавлен только в участника репозитория!", NotificationCriticalLevel.Error));
            }
        }
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in ParentRepository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            Name = NamingHelper.GetNewName(existNames, "Новый узел");
            Childs = new ObservableCollection<IChildren>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
