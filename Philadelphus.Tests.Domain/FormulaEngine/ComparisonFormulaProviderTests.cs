using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты системных формул и операторов сравнения.
    /// </summary>
    public class ComparisonFormulaProviderTests
    {
        /// <summary>
        /// Проверяет равенство числового выражения и числа.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_True_For_Equal_Numeric_Expressions()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("10=(5*2)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(true);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
        }

        /// <summary>
        /// Проверяет сравнение строк на равенство.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_False_For_Different_Strings()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("\"Стол\"=\"Стул\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(false);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
        }

        /// <summary>
        /// Проверяет числовое сравнение больше.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_True_For_Greater_Numeric_Value()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("3>2", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(true);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
        }

        /// <summary>
        /// Проверяет оператор неравенства.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_True_For_NotEqual_Values()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("1<>2", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(true);
            result.ValueType.Should().Be(SystemBaseType.BOOL);
        }

        /// <summary>
        /// Проверяет ошибку несовместимых типов для равенства.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_TypeMismatch_For_Incompatible_Equality_Types()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("\"1\"=1", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет ошибку несовместимых типов для сравнения порядка.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_TypeMismatch_For_Boolean_Ordering()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("(1=1)>(1=2)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет регистрацию формул сравнения по именам и операторным псевдонимам.
        /// </summary>
        [Fact]
        public void Provider_Registers_Comparison_Names_And_Aliases()
        {
            var registry = CreateRegistry();

            registry.TryResolve("РАВНО", out _).Should().BeTrue();
            registry.TryResolve("=", out _).Should().BeTrue();
            registry.TryResolve("НЕ_РАВНО", out _).Should().BeTrue();
            registry.TryResolve("<>", out _).Should().BeTrue();
            registry.TryResolve("БОЛЬШЕ", out _).Should().BeTrue();
            registry.TryResolve(">", out _).Should().BeTrue();
            registry.TryResolve("МЕНЬШЕ", out _).Should().BeTrue();
            registry.TryResolve("<", out _).Should().BeTrue();
            registry.TryResolve("БОЛЬШЕ_ИЛИ_РАВНО", out _).Should().BeTrue();
            registry.TryResolve(">=", out _).Should().BeTrue();
            registry.TryResolve("МЕНЬШЕ_ИЛИ_РАВНО", out _).Should().BeTrue();
            registry.TryResolve("<=", out _).Should().BeTrue();
        }

        /// <summary>
        /// Создает вычислитель с зарегистрированными арифметическими формулами и формулами сравнения.
        /// </summary>
        /// <returns>Вычислитель формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            return new FormulaAstEvaluator(CreateRegistry());
        }

        /// <summary>
        /// Создает реестр с формулами, нужными для тестов сравнения.
        /// </summary>
        /// <returns>Реестр формул.</returns>
        private static FormulaRegistry CreateRegistry()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new ArithmeticFormulaProvider());
            registry.RegisterProvider(new ComparisonFormulaProvider());
            return registry;
        }
    }
}

