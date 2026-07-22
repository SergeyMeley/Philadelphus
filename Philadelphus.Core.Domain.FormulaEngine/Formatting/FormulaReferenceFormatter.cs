using Philadelphus.Core.Domain.FormulaEngine.Expressions;
using Philadelphus.Core.Domain.FormulaEngine.Parsing;

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

        /// <summary>
        /// Формирует короткую формулу коллекции ссылок на листья рабочего дерева.
        /// </summary>
        /// <param name="uuids">UUID листьев в порядке элементов коллекции.</param>
        /// <returns>Формула вида <c>={[uuid1];[uuid2]}</c>.</returns>
        public static string CreateTreeLeaveCollectionFormula(IEnumerable<Guid> uuids)
        {
            ArgumentNullException.ThrowIfNull(uuids);

            return $"={{{string.Join(";", uuids.Select(CreateTreeLeaveReference))}}}";
        }

        /// <summary>
        /// Преобразует формулу одиночного значения в формулу массива с одним выражением.
        /// </summary>
        /// <param name="formula">Формула одиночного значения, начинающаяся с <c>=</c>.</param>
        /// <returns>Формула вида <c>={expr}</c>.</returns>
        public static string CreateCollectionFormulaFromScalarFormula(string formula)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(formula);

            var trimmedFormula = formula.Trim();
            if (trimmedFormula.StartsWith('=') == false || trimmedFormula.Length == 1)
                throw new ArgumentException("Формула должна начинаться с '=' и содержать выражение.", nameof(formula));

            return $"={{{trimmedFormula[1..]}}}";
        }

        /// <summary>
        /// Извлекает единственное выражение формулы массива как формулу одиночного значения.
        /// </summary>
        public static bool TryCreateScalarFormulaFromCollectionFormula(
            string? formula,
            out string scalarFormula)
        {
            scalarFormula = string.Empty;
            if (string.IsNullOrWhiteSpace(formula))
                return true;

            var trimmedFormula = formula.Trim();
            var parseResult = FormulaParser.Parse(trimmedFormula);
            if (parseResult.IsSuccess == false
                || parseResult.Expression is not FunctionCallFormulaExpression collection
                || string.Equals(collection.Name, "МАССИВ", StringComparison.OrdinalIgnoreCase) == false
                || collection.Arguments.Count > 1)
            {
                return false;
            }

            if (collection.Arguments.Count == 0)
                return true;

            var expressionSpan = collection.Arguments[0].Span;
            scalarFormula = $"={trimmedFormula.Substring(expressionSpan.Start, expressionSpan.Length)}";
            return true;
        }

        /// <summary>
        /// Формирует относительную ссылку на атрибут текущего элемента.
        /// </summary>
        /// <param name="attributeName">Наименование атрибута.</param>
        /// <returns>Выражение вида <c>АТРИБУТ("Наименование")</c>.</returns>
        public static string CreateRelativeAttributeReference(string? attributeName)
        {
            var escapedName = (attributeName ?? string.Empty)
                .Replace("\"", "\"\"", StringComparison.Ordinal);

            return $"АТРИБУТ(\"{escapedName}\")";
        }
    }
}
