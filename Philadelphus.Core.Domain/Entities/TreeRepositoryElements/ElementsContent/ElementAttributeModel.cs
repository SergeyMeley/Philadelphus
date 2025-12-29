using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure;
using Philadelphus.Core.Domain.Entities.OtherEntities;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent
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
