using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
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
                    mainEntity = new TreeRepository("Новый репозиторий", Guid.Empty);
                    break;
                case EntityTypes.Root:
                    mainEntity = new TreeRoot("Новый корень", Guid.Empty);
                    break;
                case EntityTypes.Node:
                    mainEntity = new TreeNode("Новый узел", Guid.Empty);
                    break;
                case EntityTypes.Leave:
                    mainEntity = new TreeNode("Новый лист", Guid.Empty);
                    break;
                case EntityTypes.Attribute:
                    mainEntity = new TreeNode("Новый атрибут", Guid.Empty);
                    break;
                default:
                    mainEntity = null;
                    break;
            }
            return mainEntity;
        }
    }
}
