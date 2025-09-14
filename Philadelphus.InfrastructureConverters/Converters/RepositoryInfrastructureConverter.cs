//using Philadelphus.Business.Entities.Enums;
//using Philadelphus.Business.Entities.RepositoryElements;
//using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
//using Philadelphus.Business.Factories;
//using Philadelphus.InfrastructureEntities.MainEntities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    public static class RepositoryInfrastructureConverter
//    {
//        public static TreeRepository BusinessToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (TreeRepository)BusinessToDbMainProperties((MainEntityBaseModel)businessEntity, new TreeRepository());
//            return result;
//        }
//        public static List<TreeRepository> BusinessToDbEntityCollection(this IEnumerable<TreeRepositoryModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<TreeRepository>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = businessEntity.BusinessToDbMainProperties(new TreeRepository());
//                result.Add(entity);
//            }
//            return result;
//        }
//        public static TreeRepositoryModel DbToBusinessEntity(this TreeRepository dbEntity, out TreeRepositoryModel businessEntity = null)
//        {
//            if (dbEntity == null)
//                return businessEntity;
//            result = dbEntity.DbToBusinessMainProperties((MainEntityBaseModel)businessEntity);
//            return result;
//        }
//        public static List<TreeRepositoryModel> DbToBusinessEntityCollection(this IEnumerable<TreeRepository> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<TreeRepositoryModel>();
//            foreach (var dbEntity in dbEntityCollection)
//            {
//                var entity = new TreeRepositoryModel();
//                entity = dbEntityCollection.DbToBusinessMainProperties((MainEntityBaseModel)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypesModel.Repository, Guid.Empty));
//                result.Add(entity);
//            }
//            return result;
//        }
//    }
//}
