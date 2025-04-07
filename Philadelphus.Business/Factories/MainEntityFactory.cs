using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.Interfaces;
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
                    mainEntity = new TreeRepository(Guid.Empty);
                    mainEntity.Name = "Новый репозиторий";
                    break;
                case EntityTypes.Root:
                    mainEntity = new TreeRoot(Guid.Empty);
                    mainEntity.Name = "Новый корень";
                    break;
                case EntityTypes.Node:
                    mainEntity = new TreeNode(Guid.Empty);
                    mainEntity.Name = "Новый узел";
                    break;
                case EntityTypes.Leave:
                    mainEntity = new TreeNode(Guid.Empty);
                    mainEntity.Name = "Новый лист";
                    break;
                case EntityTypes.Attribute:
                    mainEntity = new TreeNode(Guid.Empty);
                    mainEntity.Name = "Новый атрибут";
                    break;
                default:
                    mainEntity = null;
                    break;
            }
            return mainEntity;
        }
    }
}
