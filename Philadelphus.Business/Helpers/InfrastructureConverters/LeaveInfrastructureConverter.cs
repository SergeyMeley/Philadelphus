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

//namespace Philadelphus.Business.Helpers.InfrastructureConverters
//{
//    public static class LeaveInfrastructureConverter
//    {
//        public static TreeLeave ToDbEntity(TreeLeaveModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (TreeLeave)businessEntity.ToDbEntityGeneralProperties(new TreeLeave());
//            return result;
//        }
//        public static List<TreeLeave> ToDbEntityCollection(IEnumerable<TreeLeaveModel> businessEntityCollection)
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
//        public static TreeLeaveModel ToModel(IMainEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new TreeLeave(new Guid(dbEntity.ParentGuid));
//            //result = (TreeLeave)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
//            //return result;
//            return null;
//        }
//        public static List<TreeLeaveModel> ToModelCollection(IEnumerable<IMainEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<TreeLeaveModel>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    var entity = new TreeLeave(new Guid(dbEntity.ParentGuid));
//            //    entity = (TreeLeave)ToModelGeneralProperties(dbEntity, (TreeRootMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
//            //    result.Add(entity);
//            //}
//            return result;
//        }
//    }
//}
