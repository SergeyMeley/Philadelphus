//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    public static class AttributeEntryInfrastructureConverter
//    {
//        internal IEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (AttributeEntry)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeEntry());
//            result.Guid = businessEntity.Guid;
//            return result;
//        }
//        internal override IEnumerable<IEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<AttributeEntry>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (AttributeEntry)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeEntry());
//                entity.Guid = businessEntity.Guid;
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override IMainEntityModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            ////var result = new EntityAttributeEntry();
//            //result = (EntityAttributeEntry)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            //var result = new List<EntityAttributeEntry>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    //var entity = new EntityAttributeEntry());
//            //    //entity = (EntityAttributeEntry)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //    //result.Add(entity);
//            //}
//            //return result;
//            return null;
//        }
//    }
//}
