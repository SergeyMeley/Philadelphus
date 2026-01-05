using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
{
    public class TreeLeaveModel : TreeRootMemberBaseModel, IChildrenModel, ITreeRootMemberModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Leave; }
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }
        internal TreeLeaveModel(Guid uuid, TreeNodeModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
        {
            if (SetParents(parent))
            {
                Initialize();
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
