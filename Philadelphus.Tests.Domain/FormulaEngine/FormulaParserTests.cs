using FluentAssertions;
using Philadelphus.Core.Domain.FormulaEngine.Expressions;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты синтаксического анализа формул.
    /// </summary>
    public class FormulaParserTests
    {
        /// <summary>
        /// Проверяет разбор простого вызова функции.
        /// </summary>
        [Fact]
        public void Parse_Builds_Function_Call()
        {
            var result = FormulaParser.Parse("СУММ(1;2)");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<FunctionCallFormulaExpression>().Subject;
            expression.Name.Should().Be("СУММ");
            expression.Arguments.Should().HaveCount(2);
        }

        /// <summary>
        /// Проверяет разбор вложенных вызовов функций.
        /// </summary>
        [Fact]
        public void Parse_Builds_Nested_Function_Calls()
        {
            var result = FormulaParser.Parse("СУММ(ПРОИЗВ(2;3);КОРЕНЬ(4))");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<FunctionCallFormulaExpression>().Subject;
            expression.Arguments[0].Should().BeOfType<FunctionCallFormulaExpression>()
                .Which.Name.Should().Be("ПРОИЗВ");
            expression.Arguments[1].Should().BeOfType<FunctionCallFormulaExpression>()
                .Which.Name.Should().Be("КОРЕНЬ");
        }

        /// <summary>
        /// Проверяет приоритет умножения над сложением.
        /// </summary>
        [Fact]
        public void Parse_Respects_Multiplication_Precedence()
        {
            var result = FormulaParser.Parse("1+2*4");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<BinaryFormulaExpression>().Subject;
            expression.Operator.Should().Be("+");
            expression.Right.Should().BeOfType<BinaryFormulaExpression>()
                .Which.Operator.Should().Be("*");
        }

        /// <summary>
        /// Проверяет правоассоциативность оператора степени.
        /// </summary>
        [Fact]
        public void Parse_Makes_Power_Right_Associative()
        {
            var result = FormulaParser.Parse("2^3^4");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<BinaryFormulaExpression>().Subject;
            expression.Operator.Should().Be("^");
            expression.Right.Should().BeOfType<BinaryFormulaExpression>()
                .Which.Operator.Should().Be("^");
        }

        /// <summary>
        /// Проверяет изменение приоритета через скобки.
        /// </summary>
        [Fact]
        public void Parse_Respects_Parentheses()
        {
            var result = FormulaParser.Parse("(1+2)*4");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<BinaryFormulaExpression>().Subject;
            expression.Operator.Should().Be("*");
            expression.Left.Should().BeOfType<BinaryFormulaExpression>()
                .Which.Operator.Should().Be("+");
        }

        /// <summary>
        /// Проверяет разбор условного оператора.
        /// </summary>
        [Fact]
        public void Parse_Builds_Conditional_Expression()
        {
            var result = FormulaParser.Parse("1=1?\"Да\":\"Нет\"");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<ConditionalFormulaExpression>().Subject;
            expression.Condition.Should().BeOfType<BinaryFormulaExpression>()
                .Which.Operator.Should().Be("=");
            expression.WhenTrue.Should().BeOfType<LiteralFormulaExpression>()
                .Which.Value.Should().Be("Да");
            expression.WhenFalse.Should().BeOfType<LiteralFormulaExpression>()
                .Which.Value.Should().Be("Нет");
        }

        /// <summary>
        /// Проверяет разбор ссылки на лист по UUID.
        /// </summary>
        [Fact]
        public void Parse_Builds_TreeLeave_Reference()
        {
            var uuid = Guid.Parse("9769db19-89a2-49ed-a3fd-f1e539a652b1");

            var result = FormulaParser.Parse($"[{uuid}]");

            result.IsSuccess.Should().BeTrue();
            result.Expression.Should().BeOfType<TreeLeaveReferenceFormulaExpression>()
                .Which.Uuid.Should().Be(uuid);
        }

        /// <summary>
        /// Проверяет разбор объектного вызова СВОЙСТВО.
        /// </summary>
        [Fact]
        public void Parse_Builds_Property_Object_Method_Call()
        {
            var uuid = Guid.Parse("9769db19-89a2-49ed-a3fd-f1e539a652b1");

            var result = FormulaParser.Parse($"[{uuid}].СВОЙСТВО(Name)");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<ObjectMethodCallFormulaExpression>().Subject;
            expression.MethodName.Should().Be("СВОЙСТВО");
            expression.Target.Should().BeOfType<TreeLeaveReferenceFormulaExpression>();
            expression.Arguments.Should().ContainSingle()
                .Which.Should().BeOfType<IdentifierFormulaExpression>()
                .Which.Name.Should().Be("Name");
        }

        /// <summary>
        /// Проверяет разбор объектного вызова АТРИБУТ.
        /// </summary>
        [Fact]
        public void Parse_Builds_Attribute_Object_Method_Call()
        {
            var uuid = Guid.Parse("9769db19-89a2-49ed-a3fd-f1e539a652b1");

            var result = FormulaParser.Parse($"[{uuid}].АТРИБУТ(\"Цвет\")");

            result.IsSuccess.Should().BeTrue();
            var expression = result.Expression.Should().BeOfType<ObjectMethodCallFormulaExpression>().Subject;
            expression.MethodName.Should().Be("АТРИБУТ");
            expression.Arguments.Should().ContainSingle()
                .Which.Should().BeOfType<LiteralFormulaExpression>()
                .Which.Value.Should().Be("Цвет");
        }

        /// <summary>
        /// Проверяет ошибку при незакрытой скобке.
        /// </summary>
        [Fact]
        public void Parse_Returns_Error_For_Missing_Closing_Parenthesis()
        {
            var result = FormulaParser.Parse("СУММ(1;2");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }
}
