namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Предоставляет общую нормализацию пользовательских наименований доменных элементов.
    /// </summary>
    public static class NameNormalizationHelper
    {
        /// <summary>
        /// Нормализует имя так же, как правила записи свойства Name: удаляет запрещенные символы,
        /// обрезает края и сворачивает повторяющиеся пробелы.
        /// </summary>
        /// <param name="value">Исходное имя.</param>
        /// <returns>Имя без запрещенных символов и лишних пробелов по краям.</returns>
        public static string NormalizeName(string? value)
        {
            if (value == null)
                return string.Empty;

            var valueWithoutInvalidCharacters = new string(value.Where(x => IsInvalidNameCharacter(x) == false).ToArray());

            return CollapseSpaces(valueWithoutInvalidCharacters.Trim());
        }

        private static bool IsInvalidNameCharacter(char value)
        {
            // Список намеренно является списком запрещенных символов, а не белым списком допустимых.
            return value == '{'
                || value == '}'
                || value == '['
                || value == ']'
                || value == '~'
                || value == '&';
        }

        private static string CollapseSpaces(string value)
        {
            if (value.Length == 0)
                return value;

            var result = new System.Text.StringBuilder(value.Length);
            var previousWasSpace = false;

            foreach (var character in value)
            {
                if (character == ' ')
                {
                    if (previousWasSpace)
                    {
                        continue;
                    }

                    previousWasSpace = true;
                }
                else
                {
                    previousWasSpace = false;
                }

                result.Append(character);
            }

            return result.ToString();
        }
    }
}
