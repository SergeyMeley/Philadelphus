using AutoMapper;
using Moq;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Serilog;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Создает контекст вычисления формул с системным рабочим деревом и доменным сервисом создания листьев.
    /// </summary>
    internal static class FormulaEngineTestContextFactory
    {
        /// <summary>
        /// Создает контекст, достаточный для материализации результатов формул в системные листья.
        /// </summary>
        /// <returns>Контекст вычисления FormulaEngine.</returns>
        public static FormulaExecutionContext Create()
        {
            var notificationService = new FakeNotificationService();
            var systemBaseWorkingTree = CreateSystemBaseWorkingTree(notificationService);
            var repositoryService = new PhiladelphusRepositoryService(
                Mock.Of<IMapper>(),
                Mock.Of<ILogger>(),
                notificationService);

            return new FormulaExecutionContext
            {
                SystemBaseWorkingTree = systemBaseWorkingTree,
                TreeLeaveResolver = new WorkingTreeTreeLeaveResolver(systemBaseWorkingTree),
                RepositoryService = repositoryService,
                NotificationService = notificationService
            };
        }

        /// <summary>
        /// Создает системное рабочее дерево с узлами базовых типов и предопределенными BOOL-листьями.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений тестового домена.</param>
        /// <returns>Системное рабочее дерево.</returns>
        private static FakeWorkingTreeModel CreateSystemBaseWorkingTree(FakeNotificationService notificationService)
        {
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.NewGuid(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());

            var objectNode = CreateNode(root, tree, SystemBaseType.OBJECT, notificationService);
            CreateNode(objectNode, tree, SystemBaseType.STRING, notificationService);
            var boolNode = CreateNode(objectNode, tree, SystemBaseType.BOOL, notificationService);
            CreateNode(objectNode, tree, SystemBaseType.FILE, notificationService);

            var numericNode = CreateNode(objectNode, tree, SystemBaseType.NUMERIC, notificationService);
            CreateNode(numericNode, tree, SystemBaseType.INTEGER, notificationService);
            CreateNode(numericNode, tree, SystemBaseType.FLOAT, notificationService);
            CreateNode(numericNode, tree, SystemBaseType.MONEY, notificationService);

            var dateTimeNode = CreateNode(objectNode, tree, SystemBaseType.DATETIME, notificationService);
            CreateNode(dateTimeNode, tree, SystemBaseType.DATE, notificationService);
            CreateNode(dateTimeNode, tree, SystemBaseType.TIME, notificationService);

            CreateBoolLeave(boolNode, tree, "Истина", notificationService);
            CreateBoolLeave(boolNode, tree, "Ложь", notificationService);

            return tree;
        }

        /// <summary>
        /// Создает системный узел указанного базового типа.
        /// </summary>
        /// <param name="parent">Родительский элемент системного дерева.</param>
        /// <param name="tree">Системное рабочее дерево.</param>
        /// <param name="type">Системный базовый тип узла.</param>
        /// <param name="notificationService">Сервис уведомлений тестового домена.</param>
        /// <returns>Созданный системный узел.</returns>
        private static SystemBaseTreeNodeModel CreateNode(
            Philadelphus.Core.Domain.Interfaces.IParentModel parent,
            FakeWorkingTreeModel tree,
            SystemBaseType type,
            FakeNotificationService notificationService)
        {
            return new SystemBaseTreeNodeModel(
                parent,
                tree,
                type,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());
        }

        /// <summary>
        /// Создает предопределенный системный лист BOOL.
        /// </summary>
        /// <param name="parent">Системный узел BOOL.</param>
        /// <param name="tree">Системное рабочее дерево.</param>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <param name="notificationService">Сервис уведомлений тестового домена.</param>
        private static void CreateBoolLeave(
            SystemBaseTreeNodeModel parent,
            FakeWorkingTreeModel tree,
            string value,
            FakeNotificationService notificationService)
        {
            _ = new SystemBaseTreeLeaveModel(
                parent,
                tree,
                value,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
        }
    }
}
