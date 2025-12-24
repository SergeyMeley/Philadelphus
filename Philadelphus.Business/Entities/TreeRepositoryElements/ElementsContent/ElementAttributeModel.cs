using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Business.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Interfaces;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.TreeRepositoryElements.ElementsContent
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
        public ElementAttributeModel(Guid guid, IAttributeOwnerModel owner, IMainEntity dbEntity) : base(guid, dbEntity)
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
