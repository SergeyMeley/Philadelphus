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
//    internal class LeaveInfrastructureConverter : InfrastructureConverterBase
//    {
//        internal override TreeLeave ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (TreeLeave)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeLeave());
//            return result;
//        }
//        internal override List<TreeLeave> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<TreeLeave>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (TreeLeave)ToDbEntityGeneralProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeLeave());
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override TreeLeaveModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new TreeLeave(new Guid(dbEntity.ParentGuid));
//            //result = (TreeLeave)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
//            //return result;
//            return null;
//        }
//        internal override List<TreeLeaveModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<TreeLeaveModel>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    var entity = new TreeLeave(new Guid(dbEntity.ParentGuid));
//            //    entity = (TreeLeave)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
//            //    result.Add(entity);
//            //}
//            return result;
//        }
//    }
//}
