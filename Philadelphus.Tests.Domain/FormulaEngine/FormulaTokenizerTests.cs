using FluentAssertions;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;

namespace Philadelphus.Tests.Domain.FormulaEngine
{
    /// <summary>
    /// Тесты лексического анализа формул.
    /// </summary>
    public class FormulaTokenizerTests
    {
        /// <summary>
        /// Проверяет разбор функции с числовыми аргументами и разделителем ';'.
        /// </summary>
        [Fact]
        public void Tokenize_Recognizes_Function_Call_With_Semicolon_Arguments()
        {
            var result = FormulaTokenizer.Tokenize("=СУММ(1;2)");

            result.IsSuccess.Should().BeTrue();
            result.Tokens.Select(x => x.Kind).Should().ContainInOrder(
                FormulaTokenKind.FormulaStart,
                FormulaTokenKind.Identifier,
                FormulaTokenKind.OpenParenthesis,
                FormulaTokenKind.Number,
                FormulaTokenKind.Semicolon,
                FormulaTokenKind.Number,
                FormulaTokenKind.CloseParenthesis,
                FormulaTokenKind.End);
        }

        /// <summary>
        /// Проверяет разбор операторов сравнения, арифметики, конкатенации и условия.
        /// </summary>
        [Fact]
        public void Tokenize_Recognizes_Operators()
        {
            var result = FormulaTokenizer.Tokenize("1+2*4+2^3<>5&\"x\"?1:0>=0<=1");

            result.IsSuccess.Should().BeTrue();
            result.Tokens.Select(x => x.Kind).Should().Contain([
                FormulaTokenKind.Plus,
                FormulaTokenKind.Star,
                FormulaTokenKind.Caret,
                FormulaTokenKind.NotEqual,
                FormulaTokenKind.Ampersand,
                FormulaTokenKind.Question,
                FormulaTokenKind.Colon,
                FormulaTokenKind.GreaterOrEqual,
                FormulaTokenKind.LessOrEqual
            ]);
        }

        /// <summary>
        /// Проверяет разбор UUID-ссылки на лист.
        /// </summary>
        [Fact]
        public void Tokenize_Recognizes_TreeLeaveReference()
        {
            var uuid = Guid.Parse("9769db19-89a2-49ed-a3fd-f1e539a652b1");

            var result = FormulaTokenizer.Tokenize($"[{uuid}]");

            result.IsSuccess.Should().BeTrue();
            result.Tokens[0].Kind.Should().Be(FormulaTokenKind.TreeLeaveReference);
            result.Tokens[0].Value.Should().Be(uuid);
        }

        /// <summary>
        /// Проверяет разбор объектного вызова свойства.
        /// </summary>
        [Fact]
        public void Tokenize_Recognizes_Object_Method_Call_Tokens()
        {
            var uuid = Guid.Parse("9769db19-89a2-49ed-a3fd-f1e539a652b1");

            var result = FormulaTokenizer.Tokenize($"[{uuid}].СВОЙСТВО(Name)");

            result.IsSuccess.Should().BeTrue();
            result.Tokens.Select(x => x.Kind).Should().ContainInOrder(
                FormulaTokenKind.TreeLeaveReference,
                FormulaTokenKind.Dot,
                FormulaTokenKind.Identifier,
                FormulaTokenKind.OpenParenthesis,
                FormulaTokenKind.Identifier,
                FormulaTokenKind.CloseParenthesis);
        }

        /// <summary>
        /// Проверяет разбор строкового литерала.
        /// </summary>
        [Fact]
        public void Tokenize_Recognizes_String_Literal()
        {
            var result = FormulaTokenizer.Tokenize("\"Цвет\"");

            result.IsSuccess.Should().BeTrue();
            result.Tokens[0].Kind.Should().Be(FormulaTokenKind.String);
            result.Tokens[0].Value.Should().Be("Цвет");
        }

        /// <summary>
        /// Проверяет понятную ошибку при использовании запятой как разделителя.
        /// </summary>
        [Fact]
        public void Tokenize_Returns_ParseError_For_Comma_Separator()
        {
            var result = FormulaTokenizer.Tokenize("СУММ(1,2)");

            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Code.Should().Be(FormulaErrorCode.ParseError);
            result.Errors[0].Message.Should().Contain(";");
            result.Errors[0].Span.Should().Be(new FormulaTextSpan(6, 1));
        }

        /// <summary>
        /// Проверяет позицию токена в исходной формуле.
        /// </summary>
        [Fact]
        public void Tokenize_Preserves_Token_Positions()
        {
            var result = FormulaTokenizer.Tokenize("  СУММ");

            result.Tokens[0].Kind.Should().Be(FormulaTokenKind.Identifier);
            result.Tokens[0].Span.Should().Be(new FormulaTextSpan(2, 4));
        }
    }
}
