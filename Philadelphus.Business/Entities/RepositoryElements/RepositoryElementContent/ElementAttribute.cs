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
    public class ElementAttribute : MainEntityBase, ITreeElementContent, ITreeRootMember, ITreeRepositoryMember
    {
        public override EntityTypes EntityType { get => EntityTypes.None; }
        public ValueTypes ValueType { get; set; }
        public IEnumerable<ElementAttributeValue>? ValueList { get; set; }
        public ElementAttributeValue? AttributeValue { get; set; }
        public IContentOwner Owner { get; set; }
        public TreeRoot ParentRoot { get; set; }
        public TreeRepository ParentRepository { get; set; }
        public ElementAttribute(Guid guid, IContentOwner owner) : base(guid)
        {
            Guid = guid;
            Owner = owner;
            if (owner.GetType().IsAssignableTo(typeof(ITreeRepositoryMember)))
            {
                ParentRepository = ((ITreeRepositoryMember)owner).ParentRepository;
                if (owner.GetType().IsAssignableTo(typeof(ITreeRootMember)))
                {
                    ParentRoot = ((ITreeRootMember)owner).ParentRoot;
                }
                else
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new Notification("Атрибут может быть добавлен только участнику репозитория!", NotificationCriticalLevel.Error));
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
