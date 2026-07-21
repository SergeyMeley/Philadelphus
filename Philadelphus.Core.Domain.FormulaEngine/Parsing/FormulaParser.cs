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
        /// <summary>
        /// Токены исходной формулы.
        /// </summary>
        private readonly IReadOnlyList<FormulaToken> _tokens;

        /// <summary>
        /// Ошибки, найденные во время синтаксического анализа.
        /// </summary>
        private readonly List<FormulaError> _errors = new();

        /// <summary>
        /// Текущая позиция чтения в списке токенов.
        /// </summary>
        private int _position;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormulaParser" />.
        /// </summary>
        /// <param name="tokens">Токены исходной формулы.</param>
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

        /// <summary>
        /// Выполняет основной синтаксический анализ и проверяет, что все токены употреблены.
        /// </summary>
        /// <returns>Результат синтаксического анализа.</returns>
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

        /// <summary>
        /// Разбирает условный оператор с минимальным приоритетом.
        /// </summary>
        /// <returns>Выражение условия или более приоритетное выражение.</returns>
        private FormulaExpression ParseConditionalExpression()
        {
            var condition = ParseBinaryExpression(0);
            if (Match(FormulaTokenKind.Question) == false)
            {
                return condition;
            }

            var whenTrue = ParseConditionalExpression();
            Consume(FormulaTokenKind.Colon, "Условный оператор должен содержать ':'.");
            var whenFalse = ParseConditionalExpression();

            return new ConditionalFormulaExpression(
                condition,
                whenTrue,
                whenFalse,
                Merge(condition.Span, whenFalse.Span));
        }

        /// <summary>
        /// Разбирает бинарное выражение с учетом приоритетов операторов.
        /// </summary>
        /// <param name="parentPrecedence">Приоритет родительского оператора.</param>
        /// <returns>Бинарное выражение или первичное выражение.</returns>
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

        /// <summary>
        /// Разбирает первичное выражение: литерал, идентификатор, ссылку на лист или скобочную группу.
        /// </summary>
        /// <returns>Первичное выражение с учетом последующих объектных вызовов.</returns>
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
                case FormulaTokenKind.OpenBrace:
                    expression = ParseArrayExpression();
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

        /// <summary>
        /// Разбирает идентификатор как вызов функции или как самостоятельный идентификатор.
        /// </summary>
        /// <returns>Выражение идентификатора или вызова функции.</returns>
        private FormulaExpression ParseIdentifierExpression()
        {
            var identifier = Consume(FormulaTokenKind.Identifier, "Ожидался идентификатор.");
            if (Match(FormulaTokenKind.OpenParenthesis))
            {
                var arguments = ParseArguments(FormulaTokenKind.CloseParenthesis);
                var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Вызов функции должен завершаться ')'.");

                return new FunctionCallFormulaExpression(
                    identifier.Text,
                    arguments,
                    Merge(identifier.Span, closeParenthesis.Span));
            }

            return new IdentifierFormulaExpression(identifier.Text, identifier.Span);
        }

        /// <summary>
        /// Разбирает выражение в круглых скобках.
        /// </summary>
        /// <returns>Выражение с диапазоном, расширенным до скобок.</returns>
        private FormulaExpression ParseParenthesizedExpression()
        {
            var openParenthesis = Consume(FormulaTokenKind.OpenParenthesis, "Ожидалась '('.");
            var expression = ParseConditionalExpression();
            var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Группа выражения должна завершаться ')'.");

            return expression with { Span = Merge(openParenthesis.Span, closeParenthesis.Span) };
        }

        /// <summary>
        /// Разбирает короткую запись массива в фигурных скобках как вызов функции МАССИВ.
        /// </summary>
        private FormulaExpression ParseArrayExpression()
        {
            var openBrace = Consume(FormulaTokenKind.OpenBrace, "Ожидалась '{'.");
            var arguments = ParseArguments(FormulaTokenKind.CloseBrace);
            var closeBrace = Consume(
                FormulaTokenKind.CloseBrace,
                "Массив должен завершаться '}'.");

            return new FunctionCallFormulaExpression(
                "МАССИВ",
                arguments,
                Merge(openBrace.Span, closeBrace.Span));
        }

        /// <summary>
        /// Разбирает постфиксные объектные вызовы вида target.МЕТОД(...).
        /// </summary>
        /// <param name="target">Выражение, от которого вызывается объектная функция.</param>
        /// <returns>Исходное выражение или цепочка объектных вызовов.</returns>
        private FormulaExpression ParsePostfixExpression(FormulaExpression target)
        {
            var expression = target;

            while (Match(FormulaTokenKind.Dot))
            {
                var methodName = Consume(FormulaTokenKind.Identifier, "После '.' должно идти имя объектной функции.");
                Consume(FormulaTokenKind.OpenParenthesis, "Объектная функция должна содержать список аргументов в скобках.");
                var arguments = ParseArguments(FormulaTokenKind.CloseParenthesis);
                var closeParenthesis = Consume(FormulaTokenKind.CloseParenthesis, "Объектная функция должна завершаться ')'.");

                expression = new ObjectMethodCallFormulaExpression(
                    expression,
                    methodName.Text,
                    arguments,
                    Merge(expression.Span, closeParenthesis.Span));
            }

            return expression;
        }

        /// <summary>
        /// Разбирает список аргументов до закрывающей скобки.
        /// </summary>
        /// <returns>Список выражений-аргументов.</returns>
        private IReadOnlyList<FormulaExpression> ParseArguments(FormulaTokenKind closingTokenKind)
        {
            var arguments = new List<FormulaExpression>();
            if (Current.Kind == closingTokenKind)
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

        /// <summary>
        /// Текущий токен.
        /// </summary>
        private FormulaToken Current => Peek(0);

        /// <summary>
        /// Возвращает токен со смещением от текущей позиции.
        /// </summary>
        /// <param name="offset">Смещение от текущей позиции.</param>
        /// <returns>Токен со смещением или последний токен, если смещение вышло за границы.</returns>
        private FormulaToken Peek(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Count
                ? _tokens[^1]
                : _tokens[index];
        }

        /// <summary>
        /// Возвращает текущий токен и сдвигает позицию вперед.
        /// </summary>
        /// <returns>Текущий токен до сдвига.</returns>
        private FormulaToken Advance()
        {
            var current = Current;
            if (Current.Kind != FormulaTokenKind.End)
            {
                _position++;
            }

            return current;
        }

        /// <summary>
        /// Проверяет текущий токен и сдвигает позицию, если тип совпал.
        /// </summary>
        /// <param name="kind">Ожидаемый тип токена.</param>
        /// <returns>true, если токен совпал; иначе false.</returns>
        private bool Match(FormulaTokenKind kind)
        {
            if (Current.Kind != kind)
            {
                return false;
            }

            Advance();
            return true;
        }

        /// <summary>
        /// Требует токен указанного типа или добавляет ошибку.
        /// </summary>
        /// <param name="kind">Ожидаемый тип токена.</param>
        /// <param name="message">Сообщение ошибки, если тип не совпал.</param>
        /// <returns>Найденный токен или синтетический токен ожидаемого типа.</returns>
        private FormulaToken Consume(FormulaTokenKind kind, string message)
        {
            if (Current.Kind == kind)
            {
                return Advance();
            }

            AddError(message, Current);
            return new FormulaToken(kind, string.Empty, Current.Span);
        }

        /// <summary>
        /// Добавляет ошибку синтаксического анализа.
        /// </summary>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="token">Токен, к которому относится ошибка.</param>
        private void AddError(string message, FormulaToken token)
        {
            _errors.Add(new FormulaError
            {
                Code = FormulaErrorCode.ParseError,
                Message = message,
                Span = token.Span
            });
        }

        /// <summary>
        /// Возвращает приоритет бинарного оператора.
        /// </summary>
        /// <param name="kind">Тип токена оператора.</param>
        /// <returns>Приоритет оператора или 0, если токен не является бинарным оператором.</returns>
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

        /// <summary>
        /// Возвращает родительский приоритет для правой части бинарного выражения.
        /// </summary>
        /// <param name="kind">Тип токена оператора.</param>
        /// <param name="precedence">Приоритет оператора.</param>
        /// <returns>Приоритет для рекурсивного разбора правой части.</returns>
        private static int GetRightParentPrecedence(FormulaTokenKind kind, int precedence)
        {
            return kind == FormulaTokenKind.Caret
                ? precedence - 1
                : precedence;
        }

        /// <summary>
        /// Объединяет два диапазона текста в один.
        /// </summary>
        /// <param name="left">Первый диапазон.</param>
        /// <param name="right">Второй диапазон.</param>
        /// <returns>Диапазон, покрывающий оба исходных диапазона.</returns>
        private static FormulaTextSpan Merge(FormulaTextSpan left, FormulaTextSpan right)
        {
            var start = Math.Min(left.Start, right.Start);
            var end = Math.Max(left.Start + left.Length, right.Start + right.Length);
            return new FormulaTextSpan(start, end - start);
        }
    }
}
