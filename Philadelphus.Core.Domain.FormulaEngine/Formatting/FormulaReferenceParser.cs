namespace Philadelphus.Core.Domain.FormulaEngine.Formatting
{
    /// <summary>
    /// Разбирает текстовые ссылки, являющиеся частью языка формул.
    /// </summary>
    public static class FormulaReferenceParser
    {
        /// <summary>
        /// Пытается извлечь UUID листа из ссылки вида <c>[uuid]</c>.
        /// </summary>
        /// <param name="text">Текст предполагаемой ссылки.</param>
        /// <param name="uuid">UUID листа при успешном разборе.</param>
        /// <returns><see langword="true"/>, если текст является ссылкой на лист; иначе <see langword="false"/>.</returns>
        public static bool TryParseTreeLeaveReference(string text, out Guid uuid)
        {
            uuid = Guid.Empty;

            return text.Length == 38
                && text.StartsWith("[", StringComparison.Ordinal)
                && text.EndsWith("]", StringComparison.Ordinal)
                && Guid.TryParse(text[1..^1], out uuid);
        }
    }
}
