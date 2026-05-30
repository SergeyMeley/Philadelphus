using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты условной формулы ЕСЛИ и оператора ?:.
    /// </summary>
    public class ConditionalFormulaProviderTests
    {
        /// <summary>
        /// Проверяет выбор истинной ветки оператором ?:.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Operator_Returns_True_Branch()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1=1?\"Да\":\"Нет\"", new FormulaExecutionContext());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Да");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет выбор ложной ветки оператором ?:.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Operator_Returns_False_Branch()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1=2?\"Да\":\"Нет\"", new FormulaExecutionContext());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Нет");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет, что оператор ?: не вычисляет невыбранную ветку.
        /// </summary>
        [Fact]
        public void Evaluate_Conditional_Operator_Evaluates_Only_Selected_Branch()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1=1?\"Да\":ОШИБКА()", new FormulaExecutionContext());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Да");
        }

        /// <summary>
        /// Проверяет именованную формулу ЕСЛИ с тремя аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_Selected_Value()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("ЕСЛИ(1=2;\"Да\";\"Нет\")", new FormulaExecutionContext());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Нет");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет значение ИСТИНА по умолчанию для истинного условия формулы ЕСЛИ.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_Default_True_Value()
        {
            var evaluator = CreateEvaluator();
            var context = CreateContextWithSystemBaseBooleanLeaves();

            var result = evaluator.Evaluate("ЕСЛИ(10=(5*2))", context);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(true);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
            result.TreeLeave.Should().BeOfType<SystemBaseTreeLeaveModel>()
                .Which.StringValue.Should().Be("Истина");
        }

        /// <summary>
        /// Проверяет значение ЛОЖЬ по умолчанию для ложного условия формулы ЕСЛИ.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_Default_False_Value()
        {
            var evaluator = CreateEvaluator();
            var context = CreateContextWithSystemBaseBooleanLeaves();

            var result = evaluator.Evaluate("ЕСЛИ(\"Стол\"=\"Стул\")", context);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(false);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
            result.TreeLeave.Should().BeOfType<SystemBaseTreeLeaveModel>()
                .Which.StringValue.Should().Be("Ложь");
        }

        /// <summary>
        /// Проверяет, что тип результата ЕСЛИ соответствует выбранной ветке.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_Selected_Branch_Type()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("ЕСЛИ(1=1;\"Да\";0)", new FormulaExecutionContext());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Да");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет значение ЛОЖЬ по умолчанию при пропущенной ложной ветке.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_Default_False_For_Missing_False_Branch()
        {
            var evaluator = CreateEvaluator();
            var context = CreateContextWithSystemBaseBooleanLeaves();

            var result = evaluator.Evaluate("ЕСЛИ(1=2;\"Да\")", context);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(false);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
            result.TreeLeave.Should().BeOfType<SystemBaseTreeLeaveModel>()
                .Which.StringValue.Should().Be("Ложь");
        }

        /// <summary>
        /// Проверяет ошибку, если для значения по умолчанию не передано системное рабочее дерево.
        /// </summary>
        [Fact]
        public void Evaluate_Named_If_Returns_TreeLeaveNotFound_When_Default_Boolean_Is_Missing()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("ЕСЛИ(1=1)", new FormulaExecutionContext());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TreeLeaveNotFound);
        }

        /// <summary>
        /// Проверяет ошибку типа условия.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_TypeMismatch_For_Non_Boolean_Condition()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1?\"Да\":\"Нет\"", new FormulaExecutionContext());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет регистрацию ЕСЛИ и служебного псевдонима ?:.
        /// </summary>
        [Fact]
        public void Provider_Registers_If_Name_And_Conditional_Alias()
        {
            var registry = CreateRegistry();

            registry.TryResolve("ЕСЛИ", out _).Should().BeTrue();
            registry.TryResolve("?:", out _).Should().BeTrue();
        }

        /// <summary>
        /// Создает вычислитель с условными и вспомогательными формулами.
        /// </summary>
        /// <returns>Вычислитель формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            return new FormulaAstEvaluator(CreateRegistry());
        }

        /// <summary>
        /// Создает контекст вычисления с системным рабочим деревом и предопределенными BOOL-листьями.
        /// </summary>
        /// <returns>Контекст вычисления формулы.</returns>
        private static FormulaExecutionContext CreateContextWithSystemBaseBooleanLeaves()
        {
            return new FormulaExecutionContext
            {
                SystemBaseWorkingTree = CreateSystemBaseWorkingTree()
            };
        }

        /// <summary>
        /// Создает системное рабочее дерево с листами Истина и Ложь.
        /// </summary>
        /// <returns>Рабочее дерево с системными логическими значениями.</returns>
        private static FakeWorkingTreeModel CreateSystemBaseWorkingTree()
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.NewGuid(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());
            var boolNode = new SystemBaseTreeNodeModel(
                root,
                tree,
                SystemBaseType.BOOL,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());

            _ = new SystemBaseTreeLeaveModel(
                boolNode,
                tree,
                "Истина",
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
            _ = new SystemBaseTreeLeaveModel(
                boolNode,
                tree,
                "Ложь",
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());

            return tree;
        }

        /// <summary>
        /// Создает реестр с формулами, нужными для условных тестов.
        /// </summary>
        /// <returns>Реестр формул.</returns>
        private static FormulaRegistry CreateRegistry()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new ArithmeticFormulaProvider());
            registry.RegisterProvider(new ComparisonFormulaProvider());
            registry.RegisterProvider(new ConditionalFormulaProvider());
            registry.Register(new Philadelphus.Core.Domain.FormulaEngine.Contracts.FormulaDefinition
            {
                Name = "ОШИБКА",
                Evaluator = (_, _) => FormulaResult.Failure(new FormulaError
                {
                    Code = FormulaErrorCode.NotImplemented,
                    Message = "Эта ветка не должна вычисляться."
                })
            });
            return registry;
        }
    }
}
