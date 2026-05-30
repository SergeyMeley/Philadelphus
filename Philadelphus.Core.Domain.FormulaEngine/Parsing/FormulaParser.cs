using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Expressions;

namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Синтаксический анализатор формул.
    /// </summary>
    public sealed class FormulaParser
    {
        private readonly IReadOnlyList<FormulaToken> _tokens;
        private readonly List<FormulaError> _errors = new();
        private int _position;

        private FormulaParser(IReadOnlyList<FormulaToken> tokens)
        {
            _tokens = tokens;
        }

        /// <summary>
        /// Разбирает исходную строку формулы в AST.
        /// </summary>
        /// <param name="source">Исходная строка формулы.</param>
        /// <returns>Результат синтаксического анализа.</returns>
        public static FormulaParserResult Parse(string? source)
        {
            var tokenizerResult = FormulaTokenizer.Tokenize(source);
            if (tokenizerResult.IsSuccess == false)
            {
                return new FormulaParserResult(null, tokenizerResult.Errors);
            }

            return Parse(tokenizerResult.Tokens);
        }

        /// <summary>
        /// Разбирает готовый набор токенов в AST.
        /// </summary>
        /// <param name="tokens">Токены формулы.</param>
        /// <returns>Результат синтаксического анализа.</returns>
        public static FormulaParserResult Parse(IReadOnlyList<FormulaToken> tokens)
        {
            ArgumentNullException.ThrowIfNull(tokens);

            var parser = new FormulaParser(tokens);
            return parser.Parse();
        }

        private FormulaParserResult Parse()
        {
            Match(FormulaTokenKind.FormulaStart);

            var expression = ParseConditionalExpression();

            if (Current.Kind != FormulaTokenKind.End)
            {
                AddError($"Неожиданный токен '{Current.Text}'.", Current);
            }

            return new FormulaParserResult(expression, _errors);
        }

        private FormulaExpression ParseConditionalExpression()
        {
            var condition = ParseBinaryExpression(0);
            if (Match(FormulaTokenKind.Question) == false)
            {
                return condition;
            }

            var whenTrue = ParseConditionalExpression();
            var colon = Consume(FormulaTokenKind.Colon, "Условный оператор должен содержать ':'.");
            var whenFalse = ParseConditionalExpression();

            return new ConditionalFormulaExpression(
                condition,
                whenTrue,
                whenFalse,
                Merge(condition.Span, whenFalse.Span));
        }

        private FormulaExpression ParseBinaryExpression(int parentPrecedence)
        {
            var left = ParsePrimaryExpression();

            while (true)
            {
                var precedence = GetBinaryPrecedence(Current.Kind);
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                var operatorToken = Current;
                Advance();
                var right = ParseBinaryExpression(GetRightParentPrecedence(operatorToken.Kind, precedence));
                left = new BinaryFormulaExpression(
                    left,
                    operatorToken.Text,
                    right,
                    Merge(left.Span, right.Span));
            }

            return left;
        }

        private FormulaExpression ParsePrimaryExpression()
        {
            FormulaExpression expression;
            var token = Current;

            switch (token.Kind)
            {
                case FormulaTokenKind.Number:
                    Advance();
                    expression = new LiteralFormulaExpression(
                        token.Value,
                        SystemBaseType.NUMERIC,
                        token.Span);
                    break;
                case FormulaTokenKind.String:
                    Advance();
                    expression = new LiteralFormulaExpression(
                        token.Value,
                        SystemBaseType.STRING,
                        token.Span);
                    break;
                case FormulaTokenKind.Identifier:
                    expression = ParseIdentifierExpression();
                    break;
                case FormulaTokenKind.TreeLeaveReference:
                    Advance();
                    expression = new TreeLeaveReferenceFormulaExpression(
                        (Guid)token.Value!,
                        token.Span);
                    break;
                case FormulaTokenKind.OpenParenthesis:
                    expression = ParseParenthesizedExpression();
                    break;
                default:
                    AddError($"Ожидалось выражение, но получен токен '{token.Text}'.", token);
                    Advance();
                    expression = new IdentifierFormulaExpression(
                        string.Empty,
                        token.Span);
                    break;
            }

            return ParsePostfixExpression(expression);
        }

        private FormulaExpression ParseIdentifierExpression()
        {
            var identifier = Consume(FormulaTokenKind.Identifier, "Ожидался идентификатор.");
            if (Match(FormulaTokenKind.OpenParenthesis))
            {
                var arguments = ParseArguments();
                var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Вызов функции должен завершаться ')'.");

                return new FunctionCallFormulaExpression(
                    identifier.Text,
                    arguments,
                    Merge(identifier.Span, closeParenthesis.Span));
            }

            return new IdentifierFormulaExpression(identifier.Text, identifier.Span);
        }

        private FormulaExpression ParseParenthesizedExpression()
        {
            var openParenthesis = Consume(FormulaTokenKind.OpenParenthesis, "Ожидалась '('.");
            var expression = ParseConditionalExpression();
            var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Группа выражения должна завершаться ')'.");

            return expression with { Span = Merge(openParenthesis.Span, closeParenthesis.Span) };
        }

        private FormulaExpression ParsePostfixExpression(FormulaExpression target)
        {
            var expression = target;

            while (Match(FormulaTokenKind.Dot))
            {
                var methodName = Consume(FormulaTokenKind.Identifier, "После '.' должно идти имя объектной функции.");
                Consume(FormulaTokenKind.OpenParenthesis, "Объектная функция должна содержать список аргументов в скобках.");
                var arguments = ParseArguments();
                var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Объектная функция должна завершаться ')'.");

                expression = new ObjectMethodCallFormulaExpression(
                    expression,
                    methodName.Text,
                    arguments,
                    Merge(expression.Span, closeParenthesis.Span));
            }

            return expression;
        }

        private IReadOnlyList<FormulaExpression> ParseArguments()
        {
            var arguments = new List<FormulaExpression>();
            if (Current.Kind == FormulaTokenKind.CloseParenthesis)
            {
                return arguments;
            }

            while (Current.Kind != FormulaTokenKind.End)
            {
                arguments.Add(ParseConditionalExpression());

                if (Match(FormulaTokenKind.Semicolon))
                {
                    continue;
                }

                break;
            }

            return arguments;
        }

        private FormulaToken Current => Peek(0);

        private FormulaToken Peek(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Count
                ? _tokens[^1]
                : _tokens[index];
        }

        private FormulaToken Advance()
        {
            var current = Current;
            if (Current.Kind != FormulaTokenKind.End)
            {
                _position++;
            }

            return current;
        }

        private bool Match(FormulaTokenKind kind)
        {
            if (Current.Kind != kind)
            {
                return false;
            }

            Advance();
            return true;
        }

        private FormulaToken Consume(FormulaTokenKind kind, string message)
        {
            if (Current.Kind == kind)
            {
                return Advance();
            }

            AddError(message, Current);
            return new FormulaToken(kind, string.Empty, Current.Span);
        }

        private void AddError(string message, FormulaToken token)
        {
            _errors.Add(new FormulaError
            {
                Code = FormulaErrorCode.ParseError,
                Message = message,
                Span = token.Span
            });
        }

        private static int GetBinaryPrecedence(FormulaTokenKind kind)
        {
            return kind switch
            {
                FormulaTokenKind.Caret => 6,
                FormulaTokenKind.Star or FormulaTokenKind.Slash => 5,
                FormulaTokenKind.Plus or FormulaTokenKind.Minus => 4,
                FormulaTokenKind.Ampersand => 3,
                FormulaTokenKind.Equal
                    or FormulaTokenKind.NotEqual
                    or FormulaTokenKind.Greater
                    or FormulaTokenKind.Less
                    or FormulaTokenKind.GreaterOrEqual
                    or FormulaTokenKind.LessOrEqual => 2,
                _ => 0
            };
        }

        private static int GetRightParentPrecedence(FormulaTokenKind kind, int precedence)
        {
            return kind == FormulaTokenKind.Caret
                ? precedence - 1
                : precedence;
        }

        private static FormulaTextSpan Merge(FormulaTextSpan left, FormulaTextSpan right)
        {
            var start = Math.Min(left.Start, right.Start);
            var end = Math.Max(left.Start + left.Length, right.Start + right.Length);
            return new FormulaTextSpan(start, end - start);
        }
    }
}
