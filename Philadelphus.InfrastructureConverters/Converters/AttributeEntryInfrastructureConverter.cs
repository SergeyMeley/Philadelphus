//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    public static class AttributeEntryInfrastructureConverter
//    {
//        internal IEntity BusinessToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (AttributeEntry)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeEntry());
//            result.Guid = businessEntity.Guid;
//            return result;
//        }
//        internal override IEnumerable<IEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<AttributeEntry>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (AttributeEntry)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeEntry());
//                entity.Guid = businessEntity.Guid;
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override IMainEntityModel DbToBusinessEntity(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            ////var result = new EntityAttributeEntry();
//            //result = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            //var result = new List<EntityAttributeEntry>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    //var entity = new EntityAttributeEntry());
//            //    //entity = (EntityAttributeEntry)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //    //result.Add(entity);
//            //}
//            //return result;
//            return null;
//        }
//    }
//}
