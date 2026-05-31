using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты вычисления AST формул.
    /// </summary>
    public class FormulaAstEvaluatorTests
    {
        /// <summary>
        /// Проверяет вычисление числового литерала без преобразования через строку.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_Numeric_Literal()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("42", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет вычисление строкового литерала как строкового значения.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_String_Literal()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("\"123\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("123");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет вызов зарегистрированной функции с вычисленными аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Calls_Registered_Function()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "ПАРА",
                (_, arguments) => FormulaResult.Success(arguments.Count, SystemBaseType.INTEGER)));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("ПАРА(1;\"x\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет, что бинарный оператор вычисляется через alias той же зарегистрированной формулы.
        /// </summary>
        [Fact]
        public void Evaluate_Routes_Binary_Operator_Through_Formula_Alias()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "СЛОЖИТЬ",
                (_, arguments) => FormulaResult.Success(
                    (double)arguments[0].Value! + (double)arguments[1].Value!,
                    SystemBaseType.NUMERIC),
                "+"));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("1+2", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(3d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет, что успешный результат формулы материализуется в системный лист и переиспользуется повторно.
        /// </summary>
        [Fact]
        public void Evaluate_Materializes_Result_To_SystemBase_Leave()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "СЛОЖИТЬ",
                (_, arguments) => FormulaResult.Success(
                    (double)arguments[0].Value! + (double)arguments[1].Value!,
                    SystemBaseType.NUMERIC),
                "+"));
            var evaluator = new FormulaAstEvaluator(registry);
            var context = FormulaEngineTestContextFactory.Create();

            var first = evaluator.Evaluate("1+2", context);
            var second = evaluator.Evaluate("1+2", context);

            first.IsSuccess.Should().BeTrue();
            first.TreeLeave.Should().BeOfType<SystemBaseTreeLeaveModel>()
                .Which.StringValue.Should().Be("3");
            second.TreeLeave.Should().BeSameAs(first.TreeLeave);
        }

        /// <summary>
        /// Проверяет короткое вычисление условного оператора.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Evaluates_Only_Selected_Branch()
        {
            var registry = new FormulaRegistry();
            registry.Register(CreateFormula(
                "ИСТИНА",
                (_, _) => FormulaResult.Success(true, SystemBaseType.BOOL)));
            registry.Register(CreateFormula(
                "ОШИБКА",
                (_, _) => FormulaResult.Failure(new FormulaError
                {
                    Code = FormulaErrorCode.NotImplemented,
                    Message = "Эта ветка не должна вычисляться."
                })));
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("ИСТИНА()?1:ОШИБКА()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1d);
        }

        /// <summary>
        /// Проверяет ошибку типа, если условный оператор получил не BOOL.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Returns_TypeMismatch_For_Non_Boolean_Condition()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1?\"Да\":\"Нет\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет ошибку поиска неизвестной функции.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_UnknownFunction_For_Unregistered_Function()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("НЕТ()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
            result.Error.FunctionOrOperator.Should().Be("НЕТ");
        }

        /// <summary>
        /// Проверяет, что UUID-ссылка возвращает сам обычный TreeLeaveModel.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Returns_Regular_TreeLeave_Instance()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"[{treeLeave.Uuid}]", new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree
            });

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeSameAs(treeLeave);
            result.TreeLeave.Should().BeSameAs(treeLeave);
            result.ValueType.Should().Be(SystemBaseType.USER_DEFINED);
        }

        /// <summary>
        /// Проверяет ошибку поиска UUID-ссылки в рабочем дереве.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Returns_NotFound_When_Leave_Is_Missing()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"[{Guid.NewGuid()}]", new FormulaExecutionContext
            {
                WorkingTree = new FakeWorkingTreeModel()
            });

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TreeLeaveNotFound);
        }

        /// <summary>
        /// Проверяет, что UUID-ссылка вычисляется через зарегистрированную формулу ЛИСТ.
        /// </summary>
        [Fact]
        public void Evaluate_TreeLeaveReference_Requires_List_Formula()
        {
            var evaluator = new FormulaAstEvaluator(new FormulaRegistry());

            var result = evaluator.Evaluate($"[{Guid.NewGuid()}]", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.UnknownFunction);
            result.Error.FunctionOrOperator.Should().Be("ЛИСТ");
        }

        /// <summary>
        /// Проверяет зарезервированный маршрут объектных функций до реализации СВОЙСТВО/АТРИБУТ.
        /// </summary>
        [Fact]
        public void Evaluate_Object_Method_Call_Returns_NotImplemented_For_Now()
        {
            var treeLeave = CreateTreeLeave();
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate($"[{treeLeave.Uuid}].СВОЙСТВО(Name)", new FormulaExecutionContext
            {
                WorkingTree = treeLeave.OwningWorkingTree
            });

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.NotImplemented);
            result.Error.FunctionOrOperator.Should().Be("СВОЙСТВО");
        }

        /// <summary>
        /// Создает вычислитель с пустым реестром формул.
        /// </summary>
        /// <returns>Вычислитель AST формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new TreeLeaveFormulaProvider());

            return new FormulaAstEvaluator(registry);
        }

        /// <summary>
        /// Создает тестовое определение формулы.
        /// </summary>
        /// <param name="name">Имя формулы.</param>
        /// <param name="evaluator">Делегат вычисления формулы.</param>
        /// <param name="aliases">Псевдонимы формулы.</param>
        /// <returns>Тестовое определение формулы.</returns>
        private static FormulaDefinition CreateFormula(
            string name,
            FormulaEvaluator evaluator,
            params string[] aliases)
        {
            return new FormulaDefinition
            {
                Name = name,
                Aliases = aliases,
                Description = "Тестовая формула",
                Evaluator = evaluator
            };
        }

        /// <summary>
        /// Создает обычный пользовательский лист дерева.
        /// </summary>
        /// <returns>Тестовый лист дерева.</returns>
        private static TreeLeaveModel CreateTreeLeave()
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

            return new TreeLeaveModel(
                Guid.NewGuid(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
        }
    }
}

