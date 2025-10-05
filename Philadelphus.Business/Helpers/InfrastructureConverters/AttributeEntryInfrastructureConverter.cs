//using Philadelphus.Business.Entities.Enums;
//using Philadelphus.Business.Entities.RepositoryElements;
//using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
//using Philadelphus.Business.Entities.RepositoryElements.RepositoryElementContent;
//using Philadelphus.Business.Factories;
//using Philadelphus.InfrastructureEntities.MainEntities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.Business.Helpers.InfrastructureConverters
//{
//    internal static class AttributeEntryInfrastructureConverter
//    {
//        internal static IMainEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = businessEntity.ToDbEntityGeneralProperties();
//            result.Guid = businessEntity.Guid;
//            return result;
//        }
//        internal static IEnumerable<IMainEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<TreeElementAttribute>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = businessEntity.ToDbEntityGeneralProperties();
//                entity.Guid = businessEntity.Guid;
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal static IMainEntityModel ToModel(IMainEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            ////var result = new EntityAttributeEntry();
//            //result = (EntityAttributeEntry)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //return result;
//            return null;
//        }
//        internal static IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IMainEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            //var result = new List<EntityAttributeEntry>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    //var entity = new EntityAttributeEntry());
//            //    //entity = (EntityAttributeEntry)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeEntry));
//            //    //result.Add(entity);
//            //}
//            //return result;
//            return null;
//        }
//    }
//}
