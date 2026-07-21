using System.Globalization;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Parsing
{
    /// <summary>
    /// Лексический анализатор формул.
    /// </summary>
    public sealed class FormulaTokenizer
    {
        /// <summary>
        /// Исходный текст формулы.
        /// </summary>
        private readonly string _source;

        /// <summary>
        /// Токены, найденные во время лексического анализа.
        /// </summary>
        private readonly List<FormulaToken> _tokens = new();

        /// <summary>
        /// Ошибки, найденные во время лексического анализа.
        /// </summary>
        private readonly List<FormulaError> _errors = new();

        /// <summary>
        /// Текущая позиция чтения в исходном тексте.
        /// </summary>
        private int _position;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="FormulaTokenizer" />.
        /// </summary>
        /// <param name="source">Исходный текст формулы.</param>
        private FormulaTokenizer(string source)
        {
            _source = source;
        }

        /// <summary>
        /// Разбирает исходную строку формулы на токены.
        /// </summary>
        /// <param name="source">Исходная строка формулы.</param>
        /// <returns>Результат лексического анализа.</returns>
        public static FormulaTokenizerResult Tokenize(string? source)
        {
            var tokenizer = new FormulaTokenizer(source ?? string.Empty);
            return tokenizer.Tokenize();
        }

        /// <summary>
        /// Выполняет основной цикл лексического анализа.
        /// </summary>
        /// <returns>Результат лексического анализа.</returns>
        private FormulaTokenizerResult Tokenize()
        {
            while (IsAtEnd == false)
            {
                var current = Current;

                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                if (char.IsDigit(current))
                {
                    ReadNumber();
                    continue;
                }

                if (current == '"')
                {
                    ReadString();
                    continue;
                }

                if (IsIdentifierStart(current))
                {
                    ReadIdentifier();
                    continue;
                }

                if (current == '[')
                {
                    ReadTreeLeaveReference();
                    continue;
                }

                ReadOperatorOrPunctuation();
            }

            _tokens.Add(new FormulaToken(
                FormulaTokenKind.End,
                string.Empty,
                new FormulaTextSpan(_position, 0)));

            return new FormulaTokenizerResult(_tokens, _errors);
        }

        /// <summary>
        /// Признак того, что достигнут конец исходного текста.
        /// </summary>
        private bool IsAtEnd => _position >= _source.Length;

        /// <summary>
        /// Текущий символ или '\0', если достигнут конец текста.
        /// </summary>
        private char Current => IsAtEnd ? '\0' : _source[_position];

        /// <summary>
        /// Следующий символ или '\0', если он отсутствует.
        /// </summary>
        private char Next => _position + 1 >= _source.Length ? '\0' : _source[_position + 1];

        /// <summary>
        /// Читает числовой литерал в формате invariant culture.
        /// </summary>
        private void ReadNumber()
        {
            var start = _position;

            while (char.IsDigit(Current))
            {
                _position++;
            }

            if (Current == '.' && char.IsDigit(Next))
            {
                _position++;
                while (char.IsDigit(Current))
                {
                    _position++;
                }
            }

            if ((Current == 'e' || Current == 'E')
                && (char.IsDigit(Next)
                    || ((Next == '+' || Next == '-')
                        && _position + 2 < _source.Length
                        && char.IsDigit(_source[_position + 2]))))
            {
                _position++;
                if (Current == '+' || Current == '-')
                {
                    _position++;
                }

                while (char.IsDigit(Current))
                {
                    _position++;
                }
            }

            var text = _source[start.._position];
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                AddToken(FormulaTokenKind.Number, start, _position, value);
            }
            else
            {
                AddError(
                    $"Числовой литерал '{text}' имеет неверный формат.",
                    start,
                    _position - start);
            }
        }

        /// <summary>
        /// Читает строковый литерал в двойных кавычках.
        /// </summary>
        private void ReadString()
        {
            var start = _position;
            _position++;
            var value = new List<char>();

            while (IsAtEnd == false)
            {
                if (Current == '"')
                {
                    if (Next == '"')
                    {
                        value.Add('"');
                        _position += 2;
                        continue;
                    }

                    _position++;
                    AddToken(FormulaTokenKind.String, start, _position, new string(value.ToArray()));
                    return;
                }

                value.Add(Current);
                _position++;
            }

            AddError("Строковый литерал не закрыт двойной кавычкой.", start, _position - start);
        }

        /// <summary>
        /// Читает идентификатор функции, свойства или аргумента.
        /// </summary>
        private void ReadIdentifier()
        {
            var start = _position;
            _position++;

            while (IsIdentifierPart(Current))
            {
                _position++;
            }

            AddToken(FormulaTokenKind.Identifier, start, _position);
        }

        /// <summary>
        /// Читает ссылку на лист дерева в формате [uuid].
        /// </summary>
        private void ReadTreeLeaveReference()
        {
            var start = _position;
            _position++;
            var contentStart = _position;

            while (IsAtEnd == false && Current != ']')
            {
                _position++;
            }

            if (IsAtEnd)
            {
                AddError("Ссылка на лист не закрыта символом ']'.", start, _position - start);
                return;
            }

            var uuidText = _source[contentStart.._position].Trim();
            _position++;

            if (Guid.TryParse(uuidText, out var uuid))
            {
                AddToken(FormulaTokenKind.TreeLeaveReference, start, _position, uuid);
                return;
            }

            AddError(
                $"Ссылка на лист '[{uuidText}]' должна содержать UUID.",
                start,
                _position - start);
        }

        /// <summary>
        /// Читает оператор или пунктуационный символ формулы.
        /// </summary>
        private void ReadOperatorOrPunctuation()
        {
            var start = _position;

            switch (Current)
            {
                case '=':
                    _position++;
                    AddToken(
                        _tokens.Count == 0 ? FormulaTokenKind.FormulaStart : FormulaTokenKind.Equal,
                        start,
                        _position);
                    break;
                case '(':
                    _position++;
                    AddToken(FormulaTokenKind.OpenParenthesis, start, _position);
                    break;
                case ')':
                    _position++;
                    AddToken(FormulaTokenKind.CloseParenthesis, start, _position);
                    break;
                case '{':
                    _position++;
                    AddToken(FormulaTokenKind.OpenBrace, start, _position);
                    break;
                case '}':
                    _position++;
                    AddToken(FormulaTokenKind.CloseBrace, start, _position);
                    break;
                case '.':
                    _position++;
                    AddToken(FormulaTokenKind.Dot, start, _position);
                    break;
                case ';':
                    _position++;
                    AddToken(FormulaTokenKind.Semicolon, start, _position);
                    break;
                case ',':
                    _position++;
                    AddError("Запятая не является разделителем аргументов. Используйте ';'.", start, 1);
                    break;
                case '+':
                    _position++;
                    AddToken(FormulaTokenKind.Plus, start, _position);
                    break;
                case '-':
                    _position++;
                    AddToken(FormulaTokenKind.Minus, start, _position);
                    break;
                case '*':
                    _position++;
                    AddToken(FormulaTokenKind.Star, start, _position);
                    break;
                case '/':
                    _position++;
                    AddToken(FormulaTokenKind.Slash, start, _position);
                    break;
                case '^':
                    _position++;
                    AddToken(FormulaTokenKind.Caret, start, _position);
                    break;
                case '&':
                    _position++;
                    AddToken(FormulaTokenKind.Ampersand, start, _position);
                    break;
                case '?':
                    _position++;
                    AddToken(FormulaTokenKind.Question, start, _position);
                    break;
                case ':':
                    _position++;
                    AddToken(FormulaTokenKind.Colon, start, _position);
                    break;
                case '<':
                    ReadLessOperator(start);
                    break;
                case '>':
                    ReadGreaterOperator(start);
                    break;
                default:
                    AddError($"Символ '{Current}' не поддерживается в формуле.", start, 1);
                    _position++;
                    break;
            }
        }

        /// <summary>
        /// Читает оператор '<', '<=' или '<>'.
        /// </summary>
        /// <param name="start">Начальная позиция оператора.</param>
        private void ReadLessOperator(int start)
        {
            _position++;
            if (Current == '>')
            {
                _position++;
                AddToken(FormulaTokenKind.NotEqual, start, _position);
                return;
            }

            if (Current == '=')
            {
                _position++;
                AddToken(FormulaTokenKind.LessOrEqual, start, _position);
                return;
            }

            AddToken(FormulaTokenKind.Less, start, _position);
        }

        /// <summary>
        /// Читает оператор '>' или '>='.
        /// </summary>
        /// <param name="start">Начальная позиция оператора.</param>
        private void ReadGreaterOperator(int start)
        {
            _position++;
            if (Current == '=')
            {
                _position++;
                AddToken(FormulaTokenKind.GreaterOrEqual, start, _position);
                return;
            }

            AddToken(FormulaTokenKind.Greater, start, _position);
        }

        /// <summary>
        /// Добавляет токен в результат лексического анализа.
        /// </summary>
        /// <param name="kind">Тип токена.</param>
        /// <param name="start">Начальная позиция токена.</param>
        /// <param name="end">Позиция сразу после конца токена.</param>
        /// <param name="value">Разобранное значение токена.</param>
        private void AddToken(FormulaTokenKind kind, int start, int end, object? value = null)
        {
            _tokens.Add(new FormulaToken(
                kind,
                _source[start..end],
                new FormulaTextSpan(start, end - start),
                value));
        }

        /// <summary>
        /// Добавляет ошибку лексического анализа.
        /// </summary>
        /// <param name="message">Сообщение ошибки.</param>
        /// <param name="start">Начальная позиция ошибки.</param>
        /// <param name="length">Длина ошибочного участка.</param>
        private void AddError(string message, int start, int length)
        {
            _errors.Add(new FormulaError
            {
                Code = FormulaErrorCode.ParseError,
                Message = message,
                Span = new FormulaTextSpan(start, length)
            });
        }

        /// <summary>
        /// Проверяет, может ли символ начинать идентификатор.
        /// </summary>
        /// <param name="value">Проверяемый символ.</param>
        /// <returns>true, если символ может начинать идентификатор; иначе false.</returns>
        private static bool IsIdentifierStart(char value)
        {
            return char.IsLetter(value) || value == '_';
        }

        /// <summary>
        /// Проверяет, может ли символ входить в идентификатор после первого символа.
        /// </summary>
        /// <param name="value">Проверяемый символ.</param>
        /// <returns>true, если символ может входить в идентификатор; иначе false.</returns>
        private static bool IsIdentifierPart(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_';
        }
    }
}
