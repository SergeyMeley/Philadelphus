using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeLeave : TreeRepositoryMemberBase, IChildren, ITreeRootMember
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public TreeRoot ParentRoot { get; private set; }
        public TreeLeave(Guid guid, IParent parent) : base(guid, parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevel.Error);
                return;
            }
            Parent = parent;
            ParentRepository = ((ITreeRepositoryMember)Parent).ParentRepository;
            ParentRoot = ((ITreeRootMember)Parent).ParentRoot;
            Guid = guid;
            Initialize();
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
            Name = NamingHelper.GetNewName(existNames, "Новый лист");
            //Childs = new ObservableCollection<IChildren>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
