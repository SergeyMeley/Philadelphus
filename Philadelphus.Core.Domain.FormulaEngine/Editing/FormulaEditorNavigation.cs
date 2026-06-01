using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;

namespace Philadelphus.Core.Domain.FormulaEngine.Editing
{
    /// <summary>
    /// Вспомогательная логика навигации редактора формул.
    /// </summary>
    public static class FormulaEditorNavigation
    {
        /// <summary>
        /// Ищет позицию курсора рядом с парной скобкой.
        /// </summary>
        /// <param name="source">Исходный текст формулы.</param>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="matchingCaretIndex">Позиция курсора рядом с парной скобкой.</param>
        /// <returns>True, если парная скобка найдена.</returns>
        public static bool TryGetMatchingParenthesisCaretIndex(
            string? source,
            int caretIndex,
            out int matchingCaretIndex)
        {
            matchingCaretIndex = caretIndex;
            if (IsFormulaInput(source) == false || caretIndex < 0 || caretIndex > source!.Length)
            {
                return false;
            }

            var tokenizerResult = FormulaTokenizer.Tokenize(source);
            var parentheses = tokenizerResult.Tokens
                .Where(token => token.Kind is FormulaTokenKind.OpenParenthesis or FormulaTokenKind.CloseParenthesis)
                .ToArray();
            var currentIndex = Array.FindIndex(
                parentheses,
                token => token.Span.Start == caretIndex || token.Span.Start + token.Span.Length == caretIndex);

            if (currentIndex < 0)
            {
                return false;
            }

            var current = parentheses[currentIndex];
            var pairIndex = current.Kind == FormulaTokenKind.OpenParenthesis
                ? FindClosingParenthesis(parentheses, currentIndex)
                : FindOpeningParenthesis(parentheses, currentIndex);

            if (pairIndex < 0)
            {
                return false;
            }

            matchingCaretIndex = parentheses[pairIndex].Kind == FormulaTokenKind.OpenParenthesis
                ? parentheses[pairIndex].Span.Start
                : parentheses[pairIndex].Span.Start + parentheses[pairIndex].Span.Length;
            return true;
        }

        /// <summary>
        /// Ищет диапазон текущего вызова функции.
        /// </summary>
        /// <param name="source">Исходный текст формулы.</param>
        /// <param name="caretIndex">Текущая позиция курсора.</param>
        /// <param name="selection">Диапазон текущего вызова функции.</param>
        /// <returns>True, если текущий вызов функции найден.</returns>
        public static bool TryGetCurrentFormulaCallSelection(
            string? source,
            int caretIndex,
            out FormulaTextSpan selection)
        {
            selection = default;
            if (IsFormulaInput(source) == false || caretIndex < 0 || caretIndex > source!.Length)
            {
                return false;
            }

            var tokenizerResult = FormulaTokenizer.Tokenize(source);
            var tokens = tokenizerResult.Tokens
                .Where(token => token.Kind != FormulaTokenKind.End)
                .ToArray();
            var activeCalls = new Stack<FormulaCallRange>();

            for (var index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];
                if (token.Span.Start >= caretIndex)
                {
                    break;
                }

                if (token.Kind == FormulaTokenKind.OpenParenthesis)
                {
                    activeCalls.Push(CreateCallRange(tokens, index));
                    continue;
                }

                if (token.Kind == FormulaTokenKind.CloseParenthesis && activeCalls.Count > 0)
                {
                    activeCalls.Pop();
                }
            }

            var activeCall = activeCalls.FirstOrDefault(call => string.IsNullOrWhiteSpace(call.Name) == false);
            if (activeCall is null)
            {
                return false;
            }

            var closeParenthesisIndex = activeCall.CloseParenthesisIndex >= 0
                ? activeCall.CloseParenthesisIndex
                : caretIndex - 1;
            var length = closeParenthesisIndex - activeCall.NameStartIndex + 1;
            if (length <= 0)
            {
                return false;
            }

            selection = new FormulaTextSpan(activeCall.NameStartIndex, length);
            return true;
        }

        /// <summary>
        /// Проверяет, что строка выглядит как ввод формулы.
        /// </summary>
        private static bool IsFormulaInput(string? source)
        {
            return string.IsNullOrWhiteSpace(source) == false
                && source.TrimStart().StartsWith("=", StringComparison.Ordinal);
        }

        /// <summary>
        /// Создает диапазон вызова функции по открывающей скобке.
        /// </summary>
        private static FormulaCallRange CreateCallRange(IReadOnlyList<FormulaToken> tokens, int openParenthesisTokenIndex)
        {
            var nameToken = FindNameTokenBeforeOpenParenthesis(tokens, openParenthesisTokenIndex);
            var closeParenthesisTokenIndex = FindClosingParenthesis(tokens, openParenthesisTokenIndex);
            return new FormulaCallRange(
                nameToken?.Text ?? string.Empty,
                nameToken?.Span.Start ?? tokens[openParenthesisTokenIndex].Span.Start,
                tokens[openParenthesisTokenIndex].Span.Start,
                closeParenthesisTokenIndex >= 0 ? tokens[closeParenthesisTokenIndex].Span.Start : -1);
        }

        /// <summary>
        /// Ищет токен имени функции перед открывающей скобкой.
        /// </summary>
        private static FormulaToken? FindNameTokenBeforeOpenParenthesis(
            IReadOnlyList<FormulaToken> tokens,
            int openParenthesisTokenIndex)
        {
            var previousTokenIndex = openParenthesisTokenIndex - 1;
            return previousTokenIndex >= 0 && tokens[previousTokenIndex].Kind == FormulaTokenKind.Identifier
                ? tokens[previousTokenIndex]
                : null;
        }

        /// <summary>
        /// Находит закрывающую скобку для открывающей.
        /// </summary>
        private static int FindClosingParenthesis(IReadOnlyList<FormulaToken> tokens, int openIndex)
        {
            var depth = 0;
            for (var index = openIndex; index < tokens.Count; index++)
            {
                if (tokens[index].Kind == FormulaTokenKind.OpenParenthesis)
                {
                    depth++;
                    continue;
                }

                if (tokens[index].Kind != FormulaTokenKind.CloseParenthesis)
                {
                    continue;
                }

                depth--;
                if (depth == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Находит открывающую скобку для закрывающей.
        /// </summary>
        private static int FindOpeningParenthesis(IReadOnlyList<FormulaToken> tokens, int closeIndex)
        {
            var depth = 0;
            for (var index = closeIndex; index >= 0; index--)
            {
                if (tokens[index].Kind == FormulaTokenKind.CloseParenthesis)
                {
                    depth++;
                    continue;
                }

                if (tokens[index].Kind != FormulaTokenKind.OpenParenthesis)
                {
                    continue;
                }

                depth--;
                if (depth == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Диапазон открытого вызова функции.
        /// </summary>
        /// <param name="Name">Имя функции.</param>
        /// <param name="NameStartIndex">Начальная позиция имени функции.</param>
        /// <param name="OpenParenthesisIndex">Позиция открывающей скобки.</param>
        /// <param name="CloseParenthesisIndex">Позиция закрывающей скобки или -1, если она еще не введена.</param>
        private sealed record FormulaCallRange(
            string Name,
            int NameStartIndex,
            int OpenParenthesisIndex,
            int CloseParenthesisIndex);
    }
}
