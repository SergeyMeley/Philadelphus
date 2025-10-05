//using Philadelphus.Business.Entities.Enums;
//using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
//using Philadelphus.Business.Entities.RepositoryElements;
//using Philadelphus.Business.Factories;
//using Philadelphus.InfrastructureEntities.MainEntities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.InfrastructureConverters.Converters
//{
//    internal class RootInfrastructureConverter :InfrastructureConverterBase
//    {
//        internal override IEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (TreeRoot)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeRoot());
//            return result;
//        }
//        internal override IEnumerable<IEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<TreeRoot>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (TreeRoot)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeRoot());
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override IMainEntityModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new TreeRoot(new Guid(dbEntity.ParentGuid));
//            //result = (TreeRoot)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Root));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<TreeRootModel>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    var entity = new TreeRoot(new Guid(dbEntity.ParentGuid));
//            //    entity = (TreeRoot)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Root));
//            //    result.Add(entity);
//            //}
//            return result;
//        }
//    }
//}
