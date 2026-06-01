using FluentAssertions;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты resolver'а листьев рабочего дерева для формул.
    /// </summary>
    public class WorkingTreeTreeLeaveResolverTests
    {
        /// <summary>
        /// Проверяет поиск листа по UUID.
        /// </summary>
        [Fact]
        public void ResolveByUuid_Returns_TreeLeave()
        {
            var treeLeave = CreateTreeLeave("Лист");
            var resolver = new WorkingTreeTreeLeaveResolver(treeLeave.OwningWorkingTree);

            var result = resolver.ResolveByUuid(treeLeave.Uuid);

            result.IsResolved.Should().BeTrue();
            result.TreeLeave.Should().BeSameAs(treeLeave);
        }

        /// <summary>
        /// Проверяет поиск листа по наименованию.
        /// </summary>
        [Fact]
        public void ResolveByName_Returns_TreeLeave()
        {
            var treeLeave = CreateTreeLeave("Лист");
            var resolver = new WorkingTreeTreeLeaveResolver(treeLeave.OwningWorkingTree);

            var result = resolver.ResolveByName("лист");

            result.IsResolved.Should().BeTrue();
            result.TreeLeave.Should().BeSameAs(treeLeave);
        }

        /// <summary>
        /// Проверяет ошибку для отсутствующего UUID.
        /// </summary>
        [Fact]
        public void ResolveByUuid_Returns_NotFound_For_Missing_Leave()
        {
            var resolver = new WorkingTreeTreeLeaveResolver(new FakeWorkingTreeModel());

            var result = resolver.ResolveByUuid(Guid.NewGuid());

            result.IsResolved.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TreeLeaveNotFound);
        }

        /// <summary>
        /// Проверяет зарезервированную ошибку поиска по пользовательскому коду.
        /// </summary>
        [Fact]
        public void ResolveByUserCode_Returns_NotImplemented()
        {
            var resolver = new WorkingTreeTreeLeaveResolver(new FakeWorkingTreeModel());

            var result = resolver.ResolveByUserCode("CODE");

            result.IsResolved.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
        }

        /// <summary>
        /// Проверяет зарезервированную ошибку поиска по псевдониму.
        /// </summary>
        [Fact]
        public void ResolveByAlias_Returns_NotImplemented()
        {
            var resolver = new WorkingTreeTreeLeaveResolver(new FakeWorkingTreeModel());

            var result = resolver.ResolveByAlias("alias");

            result.IsResolved.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
        }

        /// <summary>
        /// Создает обычный пользовательский лист с заданным наименованием.
        /// </summary>
        /// <param name="name">Наименование листа.</param>
        /// <returns>Тестовый лист дерева.</returns>
        private static TreeLeaveModel CreateTreeLeave(string name)
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.NewGuid(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());
            var treeLeave = new TreeLeaveModel(
                Guid.NewGuid(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());

            treeLeave.Name = name;
            return treeLeave;
        }
    }
}
