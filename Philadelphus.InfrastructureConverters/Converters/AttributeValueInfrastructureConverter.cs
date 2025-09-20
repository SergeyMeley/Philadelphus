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
//    internal class AttributeValueInfrastructureConverter : InfrastructureConverterBase
//    {
//        internal override IEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (AttributeValue)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeValue());
//            return result;
//        }
//        internal override IEnumerable<IEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<AttributeValue>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (AttributeValue)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new AttributeValue());
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override IMainEntityModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new EntityAttributeValue(dbEntity.Parent);
//            //result = (EntityAttributeValue)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<ElementAttributeValueModel>();
//            //    foreach (var dbEntity in dbEntityCollection)
//            //    {
//            //        var entity = new EntityAttributeValue(dbEntity.Parent);
//            //        entity = (EntityAttributeValue)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.AttributeValue));
//            //        result.Add(entity);
//            //    }
//            return result;
//        }
//    }
//}
