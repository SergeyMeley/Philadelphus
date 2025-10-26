using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
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
    public class TreeNodeModel : TreeRepositoryMemberBaseModel, IParentModel, ITreeRootMemberModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый узел"; }
        public override EntityTypesModel EntityType { get => EntityTypesModel.Node; }
        public EntityElementTypeModel ElementType { get; set; }
        public List<TreeNodeModel> ChildTreeNodes { get => Childs.Where(x => x.GetType() == typeof(TreeNodeModel)).Cast<TreeNodeModel>().ToList(); }
        public List<TreeLeaveModel> ChildTreeLeaves { get => Childs.Where(x => x.GetType() == typeof(TreeLeaveModel)).Cast<TreeLeaveModel>().ToList(); }
        public List<IChildrenModel> Childs { get; set; } = new List<IChildrenModel>();
        //{ 
        //    get
        //    {
        //        var result = new List<IChildrenModel>();
        //        if (ChildTreeNodes != null)
        //            result.AddRange(ChildTreeNodes);
        //        if (ChildTreeLeaves != null)
        //            result.AddRange(ChildTreeLeaves);
        //        return result;
        //    }
        //    set
        //    {
        //        if (value != null)
        //        {
        //            foreach (var item in value)
        //            {
        //                if (item.GetType() == typeof(TreeNodeModel))
        //                {
        //                    ChildTreeNodes.Add((TreeNodeModel)item);
        //                }
        //                else if (item.GetType() == typeof(TreeLeaveModel))
        //                {
        //                    ChildTreeLeaves.Add((TreeLeaveModel)item);
        //                }
        //            }
        //        }
        //    }
        //}
        public TreeRootModel ParentRoot { get; private set; }
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }
        internal TreeNodeModel(Guid guid, IParentModel parent, IMainEntity dbEntity) : base(guid, parent, dbEntity)
        {
            if (parent == null)
            {
                NotificationService.SendNotification("Не выделен родительский элемент!", NotificationCriticalLevelModel.Error);
                return;
            }
            //Guid = guid;
            //Parent = parent;
            if (parent.GetType().IsAssignableTo(typeof(ITreeRepositoryMemberModel)))
            {
                if (parent.GetType() == typeof(TreeRootModel))
                {
                    ParentRoot = (TreeRootModel)parent;
                    ParentRepository = ((TreeRootModel)parent).ParentRepository;
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
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }
    }
}
