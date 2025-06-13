using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Factories
{
    internal static class MainEntityFactory
    {
        internal static IMainEntity CreateMainEntitiesRepositoriesFactory(EntityTypes entityType)
        {
            IMainEntity mainEntity;
            switch (entityType)
            {
                case EntityTypes.None:
                    mainEntity = null;
                    break;
                case EntityTypes.Repository:
                    mainEntity = new TreeRepository(Guid.NewGuid());
                    break;
                case EntityTypes.Root:
                    mainEntity = new TreeRoot(Guid.NewGuid(), null);
                    break;
                case EntityTypes.Node:
                    mainEntity = new TreeNode(Guid.NewGuid(), null);
                    break;
                case EntityTypes.Leave:
                    mainEntity = new TreeNode(Guid.NewGuid(), null);
                    break;
                case EntityTypes.Attribute:
                    mainEntity = new TreeNode(Guid.NewGuid(), null);
                    break;
                default:
                    mainEntity = null;
                    break;
            }
            return mainEntity;
        }
    }
}
