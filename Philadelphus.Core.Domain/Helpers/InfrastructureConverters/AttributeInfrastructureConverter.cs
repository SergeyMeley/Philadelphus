using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Helpers.InfrastructureConverters
{
    public static class AttributeInfrastructureConverter
    {
        /// <summary>
        /// Конвертировать доменную модель в сущность БД
        /// </summary>
        /// <param name="businessEntity">Доменная модель</param>
        /// <returns></returns>
        public static ElementAttribute ToDbEntity(this ElementAttributeModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (ElementAttribute)businessEntity.ToDbEntityGeneralProperties(businessEntity.DbEntity);
            //result.Owner = businessEntity.Owner.DbEntity;
            result.OwnerUuid = businessEntity.Owner.Uuid;
            result.ValueTypeUuid = businessEntity.ValueType?.Uuid;
            result.ValueUuid = businessEntity.Value?.Uuid;
            return result;
        }

        /// <summary>
        /// Конвертировать коллекцию доменных моделей в коллекцию сущностей БД
        /// </summary>
        /// <param name="businessEntityCollection">Коллекция доменных моделей</param>
        /// <returns></returns>
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

        /// <summary>
        /// Конвертировать сущность БД в доменную модель
        /// </summary>
        /// <param name="dbEntity">Сущность БД</param>
        /// <param name="owner">Владелец</param>
        /// <returns></returns>
        public static ElementAttributeModel ToModel(this ElementAttribute dbEntity, IAttributeOwnerModel owner)
        {
            if (dbEntity == null)
                return null;
            var result = new ElementAttributeModel(dbEntity.Uuid, owner, dbEntity);
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
                var parent = owners.FirstOrDefault(x => x.Uuid == dbEntity.OwnerUuid);
                if (parent != null)
                {
                    result.Add(dbEntity.ToModel(parent));
                }
            }
            return result;
        }
    }
}
