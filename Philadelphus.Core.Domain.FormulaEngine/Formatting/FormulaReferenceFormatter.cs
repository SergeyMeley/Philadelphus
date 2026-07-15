namespace Philadelphus.Core.Domain.FormulaEngine.Formatting
{
    /// <summary>
    /// Формирует текстовые ссылки, являющиеся частью языка формул.
    /// </summary>
    public static class FormulaReferenceFormatter
    {
        /// <summary>
        /// Формирует выражение ссылки на лист рабочего дерева.
        /// </summary>
        /// <param name="uuid">UUID листа.</param>
        /// <returns>Ссылка вида <c>[uuid]</c>.</returns>
        public static string CreateTreeLeaveReference(Guid uuid) => $"[{uuid}]";

        /// <summary>
        /// Формирует полную формулу, возвращающую лист рабочего дерева по UUID.
        /// </summary>
        /// <param name="uuid">UUID листа.</param>
        /// <returns>Формула вида <c>=[uuid]</c>.</returns>
        public static string CreateTreeLeaveReferenceFormula(Guid uuid)
            => $"={CreateTreeLeaveReference(uuid)}";
    }
}
