using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Mapping
{
    /// <summary>
    /// Методы расширения для AutoMapper.
    /// </summary>
    public static class MappingExtensions
    {
        ///// <summary>
        ///// Игнорирует ВСЕ поля Destination, которых нет в Source
        ///// Работает даже при добавлении новых полей!
        ///// </summary>
        //public static IMappingExpression<TSource, TDest>
        //    IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        //{
        //    // 1. Берем ВСЕ свойства Destination
        //    var allDestProps = typeof(TDest).GetProperties()
        //        .Where(p => p.CanWrite) // Только те, что можно записать
        //        .Select(p => p.Name)
        //        .ToList();

        //    // 2. Берем ТОЛЬКО свойства, для которых уже настроен маппинг
        //    var configuredProps = expression.GetType()
        //        .GetProperties()
        //        .Where(p => p.Name.Contains("ForMember") || p.Name.Contains("ForPath"))
        //        .SelectMany(p => p.GetValue(expression)?.ToString()?.Split(',') ?? new string[0])
        //        .Where(name => !string.IsNullOrEmpty(name))
        //        .Select(name => name.Trim('\'', '"', ' ', '('))
        //        .Where(allDestProps.Contains)
        //        .ToHashSet();

        //    // 3. Игнорируем ВСЕ, кроме настроенных
        //    var unconfigured = allDestProps.Except(configuredProps);

        //    foreach (var prop in unconfigured)
        //        expression.ForMember(prop, opt => opt.Ignore());

        //    return expression;
        //}

        /// <summary>
        /// Смаппить хранилище данных
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="dataStorage">Хранилище данных для маппинга</param>
        /// <param name="infrastructureRepositories">Репозитории БД</param>
        /// <param name="logger">Логгер</param>
        /// <returns>Результат выполнения операции.</returns>
        public static DataStorageModel MapDataStorage(
            this IMapper mapper,
            DataStorage dataStorage,
            IEnumerable<IInfrastructureRepository> infrastructureRepositories,
            ILogger logger)
        {
            return mapper.Map<DataStorageModel>(
                dataStorage,
                opt =>
                {
                    opt.Items.Add("Repositories", infrastructureRepositories);
                    opt.Items.Add("Logger", logger);
                });
        }

        /// <summary>
        /// Смаппить репозитории
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="philadelphusRepositories">Репозитории для маппинга</param>
        /// <param name="dataStorages">Хранилища данных</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static IEnumerable<PhiladelphusRepositoryModel> MapPhiladelphusRepositories(
            this IMapper mapper,
            IEnumerable<PhiladelphusRepository> philadelphusRepositories,
            IEnumerable<IDataStorageModel> dataStorages,
            INotificationService notificationService,
            IPropertiesPolicy<PhiladelphusRepositoryModel> propertiesPolicy)
        {
            foreach (var philadelphusRepository in philadelphusRepositories)
            {
                yield return mapper.Map<PhiladelphusRepositoryModel>(
                    philadelphusRepository,
                    opt =>
                    {
                        opt.Items.Add("DataStorages", dataStorages);
                        opt.Items.Add(nameof(INotificationService), notificationService);
                        opt.Items.Add(nameof(IPropertiesPolicy<PhiladelphusRepositoryModel>), propertiesPolicy);
                    });
            }
        }

        /// <summary>
        /// Смаппить рабочие деревья
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="workingTrees">Рабочие деревья для маппинга</param>
        /// <param name="dataStorages">Хранилища данных</param>
        /// <param name="owner">Владелец</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static IEnumerable<WorkingTreeModel> MapWorkingTrees(
            this IMapper mapper,
            IEnumerable<WorkingTree> workingTrees,
            IEnumerable<IDataStorageModel> dataStorages,
            ShrubModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<WorkingTreeModel> propertiesPolicy)
        {
            return mapper.Map<List<WorkingTreeModel>>(
                workingTrees,
                opt =>
                {
                    opt.Items.Add("DataStorages", dataStorages);
                    opt.Items.Add("Owner", owner);
                    opt.Items.Add(nameof(INotificationService), notificationService);
                    opt.Items.Add(nameof(IPropertiesPolicy<WorkingTreeModel>), propertiesPolicy);
                });
        }

        /// <summary>
        /// Смаппить корень рабочего дерева
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="treeRoot">Корень для маппинга</param>
        /// <param name="owner">Владелец</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static TreeRootModel MapTreeRoot(
            this IMapper mapper,
            TreeRoot treeRoot,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeRootModel> propertiesPolicy)
        {
            return mapper.Map<TreeRootModel>(
                treeRoot,
                opt =>
                {
                    opt.Items.Add("Owner", owner);
                    opt.Items.Add(nameof(INotificationService), notificationService);
                    opt.Items.Add(nameof(IPropertiesPolicy<TreeRootModel>), propertiesPolicy);
                });
        }

        /// <summary>
        /// Смаппить узлы рабочего дерева
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="treeNodes">Узлы для маппинга</param>
        /// <param name="parents">Родители</param>
        /// <param name="owner">Владелец</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static IEnumerable<TreeNodeModel> MapTreeNodes(
            this IMapper mapper,
            IEnumerable<TreeNode> treeNodes,
            IEnumerable<IParentModel> parents,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeNodeModel> propertiesPolicy)
        {
            var correctNodes = treeNodes.Where(x => parents.Any(o => o.Uuid == (x.ParentTreeNodeUuid ?? x.ParentTreeRootUuid)));

            return mapper.Map<List<TreeNodeModel>>(
                correctNodes,
                opt =>
                {
                    opt.Items.Add("Parents", parents);
                    opt.Items.Add("Owner", owner);
                    opt.Items.Add(nameof(INotificationService), notificationService);
                    opt.Items.Add(nameof(IPropertiesPolicy<TreeNodeModel>), propertiesPolicy);
                });
        }

        /// <summary>
        /// Смаппить листы рабочего дерева
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="treeLeaves">Листы для маппинга</param>
        /// <param name="parents">Родители</param>
        /// <param name="owner">Владелец</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static IEnumerable<TreeLeaveModel> MapTreeLeaves(
            this IMapper mapper,
            IEnumerable<TreeLeave> treeLeaves,
            IEnumerable<TreeNodeModel> parents,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeLeaveModel> propertiesPolicy)
        {
            var correctLeaves = treeLeaves.Where(x => parents.Any(o => o.Uuid == x.ParentTreeNodeUuid));

            return mapper.Map<List<TreeLeaveModel>>(
                correctLeaves,
                opt =>
                {
                    opt.Items.Add("Parents", parents);
                    opt.Items.Add("Owner", owner);
                    opt.Items.Add(nameof(INotificationService), notificationService);
                    opt.Items.Add(nameof(IPropertiesPolicy<TreeLeaveModel>), propertiesPolicy);
                });
        }


        /// <summary>
        /// Смаппить атрибуты элементов
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="attributes">Атрибуты для маппинга</param>
        /// <param name="owners">Коллекция владельцев.</param>
        /// <param name="valueTypesByUuid">Все типы данных атрибутов, сгруппированные по идентификатору</param>
        /// <param name="valuesByUuid">Все значения атрибутов, сгруппированные по идентификатору</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <param name="owningWorkingTree">Владеющее рабочее дерево.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        public static IEnumerable<ElementAttributeModel> MapAttributes(
            this IMapper mapper,
            IEnumerable<ElementAttribute> attributes,
            IEnumerable<IAttributeOwnerModel> owners,
            IReadOnlyDictionary<Guid, TreeNodeModel> valueTypesByUuid,
            IReadOnlyDictionary<Guid, TreeLeaveModel> valuesByUuid,
            WorkingTreeModel owningWorkingTree,
            INotificationService notificationService,
            IPropertiesPolicy<ElementAttributeModel> propertiesPolicy)
        {
            var ownersByUuid = owners.ToDictionary(x => x.Uuid);

            var correctAttributes = attributes.Where(x => ownersByUuid.ContainsKey(x.OwnerUuid));

            return mapper.Map<List<ElementAttributeModel>>(
                correctAttributes,
                opt =>
                {
                    opt.Items.Add("OwnersByUuid", ownersByUuid);
                    opt.Items.Add("ValueTypesByUuid", valueTypesByUuid);
                    opt.Items.Add("ValuesByUuid", valuesByUuid);
                    opt.Items.Add("OwningWorkingTree", owningWorkingTree);
                    opt.Items.Add(nameof(INotificationService), notificationService);
                    opt.Items.Add(nameof(IPropertiesPolicy<ElementAttributeModel>), propertiesPolicy);
                });
        }
    }
}
