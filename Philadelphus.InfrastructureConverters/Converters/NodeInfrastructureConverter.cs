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
//    internal class NodeInfrastructureConverter : InfrastructureConverterBase
//    {
//        internal override IEntity ToDbEntity(IMainEntityModel businessEntity)
//        {
//            if (businessEntity == null)
//                return null;
//            var result = (TreeNode)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeNode());
//            return result;
//        }
//        internal override IEnumerable<IEntity> ToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
//        {
//            if (businessEntityCollection == null)
//                return null;
//            var result = new List<TreeNode>();
//            foreach (var businessEntity in businessEntityCollection)
//            {
//                var entity = (TreeNode)ToDbEntityGenetalProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeNode());
//                result.Add(entity);
//            }
//            return result;
//        }
//        internal override IMainEntityModel ToModel(IEntity dbEntity)
//        {
//            if (dbEntity == null)
//                return null;
//            //var result = new TreeNode(new Guid(dbEntity.ParentGuid));
//            //result = (TreeNode)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
//            //return result;
//            return null;
//        }
//        internal override IEnumerable<IMainEntityModel> ToModelCollection(IEnumerable<IEntity> dbEntityCollection)
//        {
//            if (dbEntityCollection == null)
//                return null;
//            var result = new List<TreeNodeModel>();
//            //foreach (var dbEntity in dbEntityCollection)
//            //{
//            //    var entity = new TreeNode(new Guid(dbEntity.ParentGuid));
//            //    entity = (TreeNode)ToModelGeneralProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
//            //    result.Add(entity);
//            //}
//            return result;
//        }
//    }
//}
