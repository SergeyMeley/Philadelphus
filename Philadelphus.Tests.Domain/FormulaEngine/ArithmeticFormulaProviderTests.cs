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
    /// Тесты арифметических системных формул и операторов.
    /// </summary>
    public class ArithmeticFormulaProviderTests
    {
        /// <summary>
        /// Проверяет вычисление выражения с приоритетами операторов.
        /// </summary>
        [Fact]
        public void Evaluate_Calculates_Arithmetic_Expression_With_Precedence()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=1+2*4+2^3", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(17L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет формулу СУММ с двумя аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Sum_Returns_Sum_Of_Two_Numbers()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=СУММ(2;3)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(5L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет формулу СУММ с вложенными числовыми выражениями.
        /// </summary>
        [Fact]
        public void Evaluate_Sum_Returns_Sum_Of_Nested_Numeric_Expressions()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=СУММ(1;ПРОИЗВ(2;4);СТЕПЕНЬ(2;3))", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(17L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет ошибку типа для нечислового аргумента СУММ.
        /// </summary>
        [Fact]
        public void Evaluate_Sum_Returns_TypeMismatch_For_Text_Argument()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=СУММ(1;\"2\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет формулу ПРОИЗВ с двумя аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Product_Returns_Product_Of_Two_Numbers()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ПРОИЗВ(2;3)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(6L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет формулу ПРОИЗВ с несколькими аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Product_Returns_Product_Of_Multiple_Numbers()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ПРОИЗВ(2;3;4)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(24L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет работу формулы ПРОИЗВ через оператор *.
        /// </summary>
        [Fact]
        public void Evaluate_Product_Works_Through_Multiply_Operator()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=2*3", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(6L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет ошибку типа для нечислового аргумента ПРОИЗВ.
        /// </summary>
        [Fact]
        public void Evaluate_Product_Returns_TypeMismatch_For_Text_Argument()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ПРОИЗВ(2;\"3\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет формулу РАЗНОСТЬ с двумя аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Difference_Returns_Left_Minus_Right()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=РАЗНОСТЬ(10;3)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(7L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет формулу ЧАСТНОЕ с двумя аргументами.
        /// </summary>
        [Fact]
        public void Evaluate_Quotient_Returns_Left_Divided_By_Right()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ЧАСТНОЕ(10;2)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(5L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет, что дробный результат деления остается типом NUMERIC.
        /// </summary>
        [Fact]
        public void Evaluate_Quotient_Returns_Numeric_For_Fractional_Result()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ЧАСТНОЕ(1;2)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0.5d);
            result.ValueType.Should().Be(SystemBaseType.NUMERIC);
        }

        /// <summary>
        /// Проверяет деление на ноль в формуле ЧАСТНОЕ.
        /// </summary>
        [Fact]
        public void Evaluate_Quotient_Returns_DivZero_For_Zero_Divisor()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ЧАСТНОЕ(1;0)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.DivZero);
            result.Error.Code.GetDisplayName().Should().Be("#ДЕЛЕНИЕ_НА_НОЛЬ");
        }

        /// <summary>
        /// Проверяет ошибку количества аргументов для бинарной формулы РАЗНОСТЬ.
        /// </summary>
        [Fact]
        public void Evaluate_Difference_Returns_InvalidArgumentCount_For_Wrong_Argument_Count()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=РАЗНОСТЬ(10;3;1)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.InvalidArgumentCount);
        }

        /// <summary>
        /// Проверяет ошибку типа для нечислового аргумента ЧАСТНОЕ.
        /// </summary>
        [Fact]
        public void Evaluate_Quotient_Returns_TypeMismatch_For_Text_Argument()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=ЧАСТНОЕ(10;\"2\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет формулу SIN с аргументом в радианах.
        /// </summary>
        [Fact]
        public void Evaluate_Sin_Returns_Sine_Of_Radians()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=SIN(0)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0d);
            result.ValueType.Should().Be(SystemBaseType.FLOAT);
        }

        /// <summary>
        /// Проверяет формулу COS с аргументом в радианах.
        /// </summary>
        [Fact]
        public void Evaluate_Cos_Returns_Cosine_Of_Radians()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=COS(0)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1d);
            result.ValueType.Should().Be(SystemBaseType.FLOAT);
        }

        /// <summary>
        /// Проверяет ошибку типа для нечислового аргумента SIN.
        /// </summary>
        [Fact]
        public void Evaluate_Sin_Returns_TypeMismatch_For_Text_Argument()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=SIN(\"0\")", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет ошибку количества аргументов для COS.
        /// </summary>
        [Fact]
        public void Evaluate_Cos_Returns_InvalidArgumentCount_For_Wrong_Argument_Count()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=COS(0;1)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.InvalidArgumentCount);
        }

        /// <summary>
        /// Проверяет формулу СТЕПЕНЬ.
        /// </summary>
        [Fact]
        public void Evaluate_Power_Returns_Left_To_Right_Power()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=СТЕПЕНЬ(3;2)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(9L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет формулу КОРЕНЬ.
        /// </summary>
        [Fact]
        public void Evaluate_SquareRoot_Returns_Root_Of_Value()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=КОРЕНЬ(4)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2d);
            result.ValueType.Should().Be(SystemBaseType.FLOAT);
        }

        /// <summary>
        /// Проверяет ошибку области значений для отрицательного аргумента КОРЕНЬ.
        /// </summary>
        [Fact]
        public void Evaluate_SquareRoot_Returns_InvalidArgumentValue_For_Negative_Value()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=КОРЕНЬ(РАЗНОСТЬ(0;1))", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.InvalidArgumentValue);
            result.Error.Code.GetDisplayName().Should().Be("#НЕКОРРЕКТНОЕ_ЗНАЧЕНИЕ");
        }

        /// <summary>
        /// Проверяет ошибку области значений для некорректной степени.
        /// </summary>
        [Fact]
        public void Evaluate_Power_Returns_InvalidArgumentValue_For_Invalid_Result()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=СТЕПЕНЬ(РАЗНОСТЬ(0;1);0.5)", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.InvalidArgumentValue);
        }

        /// <summary>
        /// Проверяет изменение приоритета арифметики через скобки.
        /// </summary>
        [Fact]
        public void Evaluate_Calculates_Parenthesized_Arithmetic_Expression()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=(1+2)*4", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(12L);
            result.ValueType.Should().Be(SystemBaseType.INTEGER);
        }

        /// <summary>
        /// Проверяет деление на ноль.
        /// </summary>
        [Fact]
        public void Evaluate_Returns_DivZero_For_Division_By_Zero()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=1/0", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.DivZero);
            result.Error.Code.GetDisplayName().Should().Be("#ДЕЛЕНИЕ_НА_НОЛЬ");
        }

        /// <summary>
        /// Проверяет строгую типизацию: строка с цифрами не считается числом.
        /// </summary>
        [Fact]
        public void Evaluate_Does_Not_Coerce_String_To_Number()
        {
            var evaluator = CreateEvaluator();

            var result = evaluator.Evaluate("=\"123\"+1", FormulaEngineTestContextFactory.Create());

            result.IsSuccess.Should().BeFalse();
            result.Error!.Code.Should().Be(FormulaErrorCode.TypeMismatch);
        }

        /// <summary>
        /// Проверяет регистрацию арифметических формул по именам и операторным псевдонимам.
        /// </summary>
        [Fact]
        public void Provider_Registers_Arithmetic_Names_And_Aliases()
        {
            var registry = CreateRegistry();

            registry.TryResolve("СУММ", out _).Should().BeTrue();
            registry.TryResolve("+", out _).Should().BeTrue();
            registry.TryResolve("РАЗНОСТЬ", out _).Should().BeTrue();
            registry.TryResolve("-", out _).Should().BeTrue();
            registry.TryResolve("ПРОИЗВ", out _).Should().BeTrue();
            registry.TryResolve("*", out _).Should().BeTrue();
            registry.TryResolve("ЧАСТНОЕ", out _).Should().BeTrue();
            registry.TryResolve("/", out _).Should().BeTrue();
            registry.TryResolve("СТЕПЕНЬ", out _).Should().BeTrue();
            registry.TryResolve("^", out _).Should().BeTrue();
            registry.TryResolve("КОРЕНЬ", out _).Should().BeTrue();
            registry.TryResolve("SIN", out _).Should().BeTrue();
            registry.TryResolve("COS", out _).Should().BeTrue();
        }

        /// <summary>
        /// Создает вычислитель с зарегистрированными арифметическими формулами.
        /// </summary>
        /// <returns>Вычислитель формул.</returns>
        private static FormulaAstEvaluator CreateEvaluator()
        {
            return new FormulaAstEvaluator(CreateRegistry());
        }

        /// <summary>
        /// Создает реестр с арифметическими формулами.
        /// </summary>
        /// <returns>Реестр формул.</returns>
        private static FormulaRegistry CreateRegistry()
        {
            var registry = new FormulaRegistry();
            registry.RegisterProvider(new ArithmeticFormulaProvider());
            return registry;
        }
    }
}

