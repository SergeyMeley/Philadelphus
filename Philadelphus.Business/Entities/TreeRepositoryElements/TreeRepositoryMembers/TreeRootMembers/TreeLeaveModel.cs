using Philadelphus.Business.Entities.ElementsProperties;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services.Implementations;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers
{
    public class TreeLeaveModel : TreeRootMemberBaseModel, IChildrenModel, ITreeRootMemberModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Leave; }
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }
        internal TreeLeaveModel(Guid guid, TreeNodeModel parent, IMainEntity dbEntity) : base(guid, parent, dbEntity)
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
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, "Новый лист");
            //Childs = new ObservableCollection<IChildren>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }
    }
}
