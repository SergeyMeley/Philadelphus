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
    public class TreeLeaveModel : TreeRepositoryMemberBaseModel, IChildrenModel, ITreeRootMemberModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Leave; }
        public TreeRootModel ParentRoot { get; private set; }
        internal TreeLeaveModel(Guid guid, IParentModel parent) : base(guid, parent)
        {
            try
            {
                if (parent == null)
                {
                    string message = "Не выделен родительский элемент!";
                    NotificationService.SendNotification(message, NotificationCriticalLeveModel.Warning, NotificationTypesModel.TextMessage);
                    throw new Exception(message);
                }
                Parent = parent;
                if (Parent.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)))
                {
                    ParentRepository = ((ITreeRepositoryMemberModel)Parent).ParentRepository;
                }
                else if (Parent.GetType() == typeof(TreeRepositoryModel))
                {
                    ParentRepository = (TreeRepositoryModel)Parent;
                }
                if (Parent.GetType().IsAssignableTo(typeof(ITreeRootMemberModel)))
                {
                    ParentRoot = ((ITreeRootMemberModel)Parent).ParentRoot;
                }
                else if (Parent.GetType() == typeof(TreeRootModel))
                {
                    ParentRoot = (TreeRootModel)Parent;
                }
                Guid = guid;
                Initialize();
            }
            catch (Exception ex)
            {
                NotificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLeveModel.Error, NotificationTypesModel.TextMessage);
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
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this);
        }
    }
}
