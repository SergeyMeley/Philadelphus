using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.RepositoryElements;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.ElementsContent;
using Philadelphus.Core.Domain.Entities.TreeRepositoryElements.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Factories;
using Philadelphus.Core.Domain.Helpers.InfrastructureConverters;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class AttributeInfrastructureConverter
    {
        public static ElementAttribute ToDbEntity(this ElementAttributeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (ElementAttribute)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            //result.Owner = businessEntity.Owner.DbEntity;
            result.OwnerUuid = businessEntity.Owner.Guid;
            result.ValueTypeUuid = businessEntity.ValueType?.Guid;
            result.ValueUuid = businessEntity.Value?.Guid;
            return result;
        }
        public static List<ElementAttribute> ToDbEntityCollection(this IEnumerable<ElementAttributeModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<ElementAttribute>();
            foreach (var businessEntity in businessEntityCollection)
            {
                result.Add(businessEntity.ToDbEntity());
            }
            return result;
        }
        public static ElementAttributeModel ToModel(this ElementAttribute dbEntity, IAttributeOwnerModel owner)
        {
            if (dbEntity == null)
                return null;
            var result = new ElementAttributeModel(dbEntity.Guid, owner, dbEntity);
            result = (ElementAttributeModel)dbEntity.ToModelGeneralProperties(result);
            return result;
        }
        public static List<ElementAttributeModel> ToModelCollection(this IEnumerable<ElementAttribute> dbEntityCollection, IEnumerable<IAttributeOwnerModel> owners)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<ElementAttributeModel>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var parent = owners.FirstOrDefault(x => x.Guid == dbEntity.OwnerUuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
