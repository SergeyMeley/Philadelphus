using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.ElementsContent
{
    /// <summary>
    /// OLD
    /// </summary>
    public class OLD_ElementAttributeModel : MainEntityBaseModel
    {
        public override EntityTypesModel EntityType { get => EntityTypesModel.None; }
        public ValueTypesModel ValueType { get; set; }
        public IEnumerable<ElementAttributeValueModel> Values { get; set; } = new List<ElementAttributeValueModel>();

        public override IDataStorageModel DataStorage => throw new NotImplementedException();

        public OLD_ElementAttributeModel(Guid guid, TreeRepositoryMemberBaseModel owner, IMainEntity dbEntity) : base(guid, dbEntity)
        {

        }
    }
}
