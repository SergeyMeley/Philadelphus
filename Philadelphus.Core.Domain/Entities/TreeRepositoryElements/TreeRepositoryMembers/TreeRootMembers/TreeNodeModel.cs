using Philadelphus.Core.Domain.Entities.ElementsProperties;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers
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
