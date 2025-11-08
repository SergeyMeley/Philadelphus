using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements.ElementProperties;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
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

namespace Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers
{
    public class TreeNodeModel : TreeRootMemberBaseModel, IParentModel, ITreeRootMemberModel
    {
        protected override string DefaultFixedPartOfName { get => "Новый узел"; }
        public override EntityTypesModel EntityType { get => EntityTypesModel.Node; }
        public EntityElementTypeModel ElementType { get; set; }
        public List<TreeNodeModel> ChildTreeNodes { get => Childs.Where(x => x.GetType() == typeof(TreeNodeModel)).Cast<TreeNodeModel>().ToList(); }
        public List<TreeLeaveModel> ChildTreeLeaves { get => Childs.Where(x => x.GetType() == typeof(TreeLeaveModel)).Cast<TreeLeaveModel>().ToList(); }
        public List<IChildrenModel> Childs { get; set; } = new List<IChildrenModel>();
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }
        internal TreeNodeModel(Guid guid, IParentModel parent, IMainEntity dbEntity) : base(guid, parent, dbEntity)
        {
            if (SetParents(parent))
            {
                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    NotificationService.SendNotification($"Произошла непредвиденная ошибка, обратитесь к разработчику. Подробности: \r\n{ex.StackTrace}", NotificationCriticalLevelModel.Error, NotificationTypesModel.TextMessage);
                    throw;
                }
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
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }
    }
}
