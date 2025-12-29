//using Philadelphus.Core.Domain.Entities.Enums;
//using Philadelphus.Core.Domain.Entities.RepositoryElements;
//using Philadelphus.Core.Domain.Entities.RepositoryElements.Interfaces;
//using Philadelphus.Core.Domain.Entities.RepositoryElements.RepositoryElementContent;
//using Philadelphus.Infrastructure.Persistence.Enums;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Philadelphus.Core.Domain.Factories
//{
//    internal static class MainEntityFactory
//    {
//        internal static IMainEntityModel CreateMainEntitiesRepositoriesFactory(EntityTypesModel entityType, Guid uuid)
//        {
//            IMainEntityModel mainEntity;
//            switch (entityType)
//            {
//                case EntityTypesModel.None:
//                    mainEntity = null;
//                    break;
//                case EntityTypesModel.Repository:
//                    mainEntity = new TreeRepositoryModel(uuid);
//                    break;
//                case EntityTypesModel.Root:
//                    mainEntity = new TreeRootModel(uuid, null);
//                    break;
//                case EntityTypesModel.Node:
//                    mainEntity = new TreeNodeModel(uuid, null);
//                    break;
//                case EntityTypesModel.Leave:
//                    mainEntity = new TreeLeaveModel(uuid, null);
//                    break;
//                case EntityTypesModel.Attribute:
//                    mainEntity = new ElementAttributeModel(uuid, null);
//                    break;
//                default:
//                    mainEntity = null;
//                    break;
//            }
//            return mainEntity;
//        }
//    }
//}
