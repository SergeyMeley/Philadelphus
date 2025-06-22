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
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.RepositoryElements
{
    public class TreeLeave : TreeRepositoryMemberBase, IChildren, ITreeRootMember
    {
        public override EntityTypes EntityType { get => EntityTypes.Leave; }
        public TreeRoot ParentRoot { get; private set; }
        internal TreeLeave(Guid guid, IParent parent) : base(guid, parent)
        {
            try
            {
                if (parent == null)
                {
                    string message = "Не выделен родительский элемент!";
                    NotificationService.SendNotification(message, NotificationCriticalLevel.Warning, NotificationTypes.TextMessage);
                    throw new Exception(message);
                }
                Parent = parent;
                if (Parent.GetType().IsAssignableTo(typeof(ITreeRepositoryMember)))
                {
                    ParentRepository = ((ITreeRepositoryMember)Parent).ParentRepository;
                }
                else if (Parent.GetType() == typeof(TreeRepository))
                {
                    ParentRepository = (TreeRepository)Parent;
                }
                if (Parent.GetType().IsAssignableTo(typeof(ITreeRootMember)))
                {
                    ParentRoot = ((ITreeRootMember)Parent).ParentRoot;
                }
                else if (Parent.GetType() == typeof(TreeRoot))
                {
                    ParentRoot = (TreeRoot)Parent;
                }
                Guid = guid;
                Initialize();
            }
            catch (Exception ex)
            {
                NotificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevel.Error, NotificationTypes.TextMessage);
                throw;
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
            Name = NamingHelper.GetNewName(existNames, "Новый лист");
            //Childs = new ObservableCollection<IChildren>();
            ElementType = new EntityElementType(Guid.NewGuid(), this);
        }
    }
}
