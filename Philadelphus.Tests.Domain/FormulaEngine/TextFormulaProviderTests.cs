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
    /// Тесты системных текстовых формул и оператора конкатенации.
    /// </summary>
    public class TextFormulaProviderTests
    {
        /// <summary>
        /// Проверяет конкатенацию строк оператором &.
        /// </summary>
        [Fact]
        public void Evaluate_Concatenates_Strings_With_Ampersand()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("\"Hello\"&\" \"&\"World\"&\"!\"", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Hello World!");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет конкатенацию чисел без обратного приведения результата к числу.
        /// </summary>
        [Fact]
        public void Evaluate_Concatenates_Numbers_As_Text()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("2&3", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("23");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет именованную формулу СЦЕПИТЬ.
        /// </summary>
        [Fact]
        public void Evaluate_Concatenates_Values_With_Named_Formula()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("СЦЕПИТЬ(\"A\";1;\"B\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("A1B");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет конкатенацию логического результата сравнения.
        /// </summary>
        [Fact]
        public void Evaluate_Concatenates_Boolean_As_Russian_Text()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("\"Результат: \"&(1=1)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Результат: Истина");
            result.ValueType.Should().Be(SystemBaseType.STRING);
        }

        /// <summary>
        /// Проверяет регистрацию текстовой формулы по имени и операторному псевдониму.
        /// </summary>
        [Fact]
        public void Provider_Registers_Text_Name_And_Alias()
        {
            var registry = CreateRegistry();

            registry.TryResolve("СЦЕПИТЬ", out _).Should().BeTrue();
            registry.TryResolve("&", out _).Should().BeTrue();
        }

        /// <summary>
        /// Проверяет ошибку несовместимого типа для прямой конкатенации ошибки/объекта.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_TypeMismatch_For_Unsupported_Text_Value()
        {
            var registry = CreateRegistry();
            registry.Register(new Philadelphus.Core.Domain.FormulaEngine.Contracts.FormulaDefinition
            {
                Name = "ОБЪЕКТ",
                Evaluator = (_, _) => FormulaResult.Success(new object(), SystemBaseType.OBJECT)
            });
            var evaluator = new FormulaAstEvaluator(registry);

            var result = evaluator.Evaluate("\"x\"&ОБЪЕКТ()", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Создает вычислитель с зарегистрированными текстовыми и вспомогательными формулами.
        /// </summary>
        /// <returns>Вычислитель формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            return new FormulaAstEvaluator(CreateRegistry());
        }

        /// <summary>
        /// Создает реестр с формулами, нужными для текстовых тестов.
        /// </summary>
        /// <returns>Реестр формул.</returns>
        private static FormulaRegistry CreateRegistry()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new TextFormulaProvider());
            registry.RegisterProvider(new ComparisonFormulaProvider());
            return registry;
        }
    }
}

