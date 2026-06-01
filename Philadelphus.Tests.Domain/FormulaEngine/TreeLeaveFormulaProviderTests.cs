using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты системной формулы ЛИСТ.
    /// </summary>
    public class TreeLeaveFormulaProviderTests
    {
        /// <summary>
        /// Проверяет регистрацию формулы ЛИСТ в реестре.
        /// </summary>
        [Fact]
        public void RegisterProvider_Registers_List_Formula()
        {
            var registry = new FormulaRegistry();

            registry.RegisterProvider(new TreeLeaveFormulaProvider());

            registry.TryResolve("ЛИСТ", out var formula).Should().BeTrue();
            formula!.Name.Should().Be("ЛИСТ");
        }

        /// <summary>
        /// Проверяет поиск листа по UUID.
        /// </summary>
        [Fact]
        public void Evaluate_List_By_Uuid_Returns_TreeLeave()
        {
            var treeLeave = CreateTreeLeave("Новый лист");
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=ЛИСТ(0;\"{treeLeave.Uuid}\")",
                CreateContext(treeLeave.OwningWorkingTree));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(treeLeave);
            result.TreeLeave.Should().BeSameAs(treeLeave);
            result.ValueType.Should().Be(SystemBaseType.USER_DEFINED);
        }

        /// <summary>
        /// Проверяет поиск листа по наименованию без учета регистра.
        /// </summary>
        [Fact]
        public void Evaluate_List_By_Name_Returns_TreeLeave()
        {
            var treeLeave = CreateTreeLeave("Новый лист");
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(1;\"новый лист\")",
                CreateContext(treeLeave.OwningWorkingTree));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(treeLeave);
            result.TreeLeave.Should().BeSameAs(treeLeave);
        }

        /// <summary>
        /// Проверяет ошибку отсутствующего листа при поиске по UUID.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_TreeLeaveNotFound_For_Missing_Uuid()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                $"=ЛИСТ(0;\"{Guid.NewGuid()}\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TreeLeaveNotFound);
        }

        /// <summary>
        /// Проверяет зарезервированную ошибку поиска по пользовательскому коду.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_NotImplemented_For_UserCode_Method()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(2;\"CODE\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
            result.Error.FunctionOrOperator.Should().Be("ЛИСТ");
        }

        /// <summary>
        /// Проверяет зарезервированную ошибку поиска по псевдониму.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_NotImplemented_For_Alias_Method()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(3;\"alias\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
            result.Error.FunctionOrOperator.Should().Be("ЛИСТ");
        }

        /// <summary>
        /// Проверяет ошибку значения для некорректного UUID.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_InvalidArgumentValue_For_Invalid_Uuid()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(0;\"not-uuid\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.InvalidArgumentValue);
        }

        /// <summary>
        /// Проверяет строгую типизацию метода поиска.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_TypeMismatch_For_Text_Method()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(\"0\";\"value\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет, что метод поиска ЛИСТ должен быть целочисленным значением.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_TypeMismatch_For_Fractional_Method()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(0.5;\"value\")",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет строгую типизацию значения поиска.
        /// </summary>
        [Fact]
        public void Evaluate_List_Returns_TypeMismatch_For_Non_Text_Value()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate(
                "=ЛИСТ(1;123)",
                CreateContext(new FakeWorkingTreeModel()));

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Создает вычислитель с зарегистрированной формулой ЛИСТ.
        /// </summary>
        /// <returns>Вычислитель формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new TreeLeaveFormulaProvider());

            return new FormulaAstEvaluator(registry);
        }

        /// <summary>
        /// Создает контекст вычисления с resolver'ом листьев.
        /// </summary>
        /// <param name="workingTree">Рабочее дерево для поиска листьев.</param>
        /// <returns>Контекст вычисления формулы.</returns>
        private static FormulaExecutionContext CreateContext(WorkingTreeModel workingTree)
        {
            return new FormulaExecutionContext
            {
                WorkingTree = workingTree,
                TreeLeaveResolver = new WorkingTreeTreeLeaveResolver(workingTree)
            };
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
