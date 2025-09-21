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
//using Philadelphus.Business.Helpers.InfrastructureConverters;

//namespace Philadelphus.Business.Helpers.InfrastructureConverters
//{
//    public static class AttributeInfrastructureConverter
//    {
//        public static ElementAttributeModel ToDbEntity(ElementAttribute businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = businessEntity.ToDbEntityGeneralProperties();
//            return result;
//        }
//        public static IEnumerable<IMainEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<ElementAttribute>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (ElementAttribute)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new ElementAttribute());
//                result.Add(entity);
//            }
//            return result;
//        }
//        public static IMainEntityModel ToModel(IMainEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new ElementAttribute(new Guid(dbEntity.ParentGuid));
//            //result = (ElementAttribute)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
//            //return result;
//            return null;
//        }
//        public static IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IMainEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<OLD_ElementAttributeModel>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    var entity = new ElementAttribute(new Guid(dbEntity.ParentGuid));
//            //    entity = (ElementAttribute)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
//            //    result.Add(entity);
//            //}
//            return result;
//        }
//    }
//}
