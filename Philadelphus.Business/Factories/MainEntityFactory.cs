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
        internal static IMainEntityModel CreateMainEntitiesRepositoriesFactory(EntityTypesModel entityType)
        {
            IMainEntityModel mainEntity;
            switch (entityType)
            {
                case EntityTypesModel.None:
                    mainEntity = null;
                    break;
                case EntityTypesModel.Repository:
                    mainEntity = new TreeRepositoryModel(Guid.NewGuid());
                    break;
                case EntityTypesModel.Root:
                    mainEntity = new TreeRootModel(Guid.NewGuid(), null);
                    break;
                case EntityTypesModel.Node:
                    mainEntity = new TreeNodeModel(Guid.NewGuid(), null);
                    break;
                case EntityTypesModel.Leave:
                    mainEntity = new TreeNodeModel(Guid.NewGuid(), null);
                    break;
                case EntityTypesModel.Attribute:
                    mainEntity = new TreeNodeModel(Guid.NewGuid(), null);
                    break;
                default:
                    mainEntity = null;
                    break;
            }
            return mainEntity;
        }
    }
}
