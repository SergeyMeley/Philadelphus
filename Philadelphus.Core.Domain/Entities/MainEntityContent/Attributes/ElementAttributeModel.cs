using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    public class ElementAttributeModel : MainEntityBaseModel, ITreeElementContentModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.Attribute; }
        public IAttributeOwnerModel Owner { get; set; }
        public override IDataStorageModel DataStorage { get => Owner.DataStorage; }
        public TreeNodeModel ValueType { get; set; }
        public IEnumerable<TreeNodeModel>? ValueTypesList { get; set; }
        public TreeLeaveModel Value { get; set; }
        public IEnumerable<TreeLeaveModel>? ValuesList { get; set; }
        public ElementAttributeModel(Guid uuid, IAttributeOwnerModel owner, IMainEntity dbEntity) : base(uuid, dbEntity)
        {
            Owner = owner;
            Initialize();
        }

        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new List<string>(), "Новый атрибут");
        }
    }
}
