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
    public class TreeNodeModel : TreeRepositoryMemberBaseModel, IParentModel, ITreeRootMemberModel, IHavingOwnDataStorageModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Node; }
        public IMainEntitiesInfrastructureRepository Infrastructure { get; private set; }
        public EntityElementTypeModel ElementType { get; set; }
        public IEnumerable<IChildrenModel> Childs { get; set; }
        public TreeRootModel ParentRoot { get; private set; }
        internal TreeNodeModel(Guid guid, IParentModel parent) : base(guid, parent)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                return;
            }
            Guid = guid;
            Parent = parent;
            if (parent.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)))
            {
                if (parent.GetType() == typeof(TreeRootModel))
                {
                    ParentRoot = (TreeRootModel)parent;

                }
                else if (parent.GetType().IsAssignableTo(typeof(ITreeRootMemberModel)))
                {
                    ParentRoot = ((ITreeRootMemberModel)parent).ParentRoot;
                    ParentRepository = ((ITreeRootMemberModel)parent).ParentRepository;
                }
                else
                {
                    NotificationService.Notifications.Add(new NotificationModel("Узел может быть добавлен только в другой узел или корень!", NotificationCriticalLevelModel.Error));
                }
                Initialize();
            }
            else
            {
                NotificationService.Notifications.Add(new NotificationModel("Узел может быть добавлен только в участника репозитория!", NotificationCriticalLevelModel.Error));
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
            Childs = new ObservableCollection<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this);
        }
    }
}
