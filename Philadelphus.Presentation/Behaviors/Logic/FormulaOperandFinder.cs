namespace Philadelphus.Presentation.Behaviors.Logic
{
    /// <summary>
    /// Чистая логика поиска операнда / ссылки-атрибута / строкового литерала в тексте формулы
    /// по позиции каретки (без привязки к UI-фреймворку). Вынесена из FormulaBarTextBoxBehavior.
    /// </summary>
    public static class FormulaOperandFinder
    {
        /// <summary>
        /// Находит вызов функции АТРИБУТ(...), внутри которого находится каретка.
        /// </summary>
        public static bool TryFindAttributeReferenceAtCaret(
            string text,
            int caretIndex,
            out int referenceStart,
            out int referenceLength)
        {
            referenceStart = 0;
            referenceLength = 0;

            const string functionName = "АТРИБУТ";
            var searchIndex = 0;

            while (searchIndex < text.Length)
            {
                var nameIndex = text.IndexOf(functionName, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (nameIndex < 0)
                {
                    return false;
                }

                var openParenthesisIndex = SkipWhiteSpace(text, nameIndex + functionName.Length);
                if (openParenthesisIndex >= text.Length
                    || text[openParenthesisIndex] != '('
                    || TryFindClosingParenthesis(text, openParenthesisIndex, out var closeParenthesisIndex) == false)
                {
                    searchIndex = nameIndex + functionName.Length;
                    continue;
                }

                var endIndex = closeParenthesisIndex + 1;
                if (caretIndex >= nameIndex && caretIndex <= endIndex)
                {
                    referenceStart = nameIndex;
                    referenceLength = endIndex - nameIndex;
                    return true;
                }

                searchIndex = endIndex;
            }

            return false;
        }

        /// <summary>
        /// Находит операнд (токен) в позиции каретки.
        /// </summary>
        public static bool TryFindOperandAtCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            if (text.Length == 0)
            {
                return false;
            }

            var tokenStart = Math.Clamp(caretIndex, 0, text.Length);
            var tokenEnd = tokenStart;

            // Двойной клик после операнда должен выделять сам операнд, а не пустое место справа от него.
            while (tokenStart > 0 && char.IsWhiteSpace(text[tokenStart - 1]))
            {
                tokenStart--;
            }

            // Строковые литералы могут содержать разделители, поэтому ищем открывающую кавычку отдельно.
            if (tokenStart > 0 && text[tokenStart - 1] == '"')
            {
                return TryFindStringLiteralBeforeCaret(text, tokenStart, out operandStart, out operandLength);
            }

            while (tokenStart > 0 && IsOperandBoundary(text[tokenStart - 1]) == false)
            {
                tokenStart--;
            }

            while (tokenEnd < text.Length && char.IsWhiteSpace(text[tokenEnd]))
            {
                tokenEnd++;
            }

            while (tokenEnd < text.Length && IsOperandBoundary(text[tokenEnd]) == false)
            {
                tokenEnd++;
            }

            if (tokenEnd <= tokenStart)
            {
                return false;
            }

            var token = text[tokenStart..tokenEnd].Trim();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            operandStart = tokenStart;
            operandLength = tokenEnd - tokenStart;
            return true;
        }

        private static bool TryFindStringLiteralBeforeCaret(
            string text,
            int caretIndex,
            out int operandStart,
            out int operandLength)
        {
            operandStart = 0;
            operandLength = 0;

            var start = caretIndex - 1;
            while (start > 0)
            {
                start--;
                if (text[start] != '"')
                {
                    continue;
                }

                var escapedQuotes = 0;
                for (var i = start + 1; i < caretIndex - 1 && text[i] == '"'; i++)
                {
                    escapedQuotes++;
                }

                if (escapedQuotes % 2 == 0)
                {
                    operandStart = start;
                    operandLength = caretIndex - start;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
        {
            closeParenthesisIndex = -1;
            var depth = 0;
            var isInString = false;

            for (var i = openParenthesisIndex; i < text.Length; i++)
            {
                var current = text[i];
                if (current == '"')
                {
                    // В формулах двойная кавычка внутри строки экранируется парой кавычек.
                    if (isInString && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        i++;
                        continue;
                    }

                    isInString = !isInString;
                    continue;
                }

                if (isInString)
                {
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                    continue;
                }

                if (current != ')')
                {
                    continue;
                }

                depth--;
                if (depth == 0)
                {
                    closeParenthesisIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static int SkipWhiteSpace(string text, int index)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
            {
                index++;
            }

            return index;
        }

        private static bool IsOperandBoundary(char value)
        {
            return char.IsWhiteSpace(value)
                || value is ';' or ',' or '+' or '-' or '*' or '/' or '(' or ')' or '>' or '<' or '=' or '!';
        }
    }
}
