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

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    internal class AttributeInfrastructureConverter : InfrastructureConverterBase
//    {
//        internal override IEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (ElementAttribute)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new ElementAttribute());
//            return result;
//        }
//        internal override IEnumerable<IEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
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
//        internal override IMainEntityModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new ElementAttribute(new Guid(dbEntity.ParentGuid));
//            //result = (ElementAttribute)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Attribute));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
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
