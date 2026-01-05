using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
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
        internal TreeNodeModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
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
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }
    }
}
