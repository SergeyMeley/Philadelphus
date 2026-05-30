using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Ссылка на лист дерева по UUID.
    /// </summary>
    /// <param name="Uuid">UUID листа дерева.</param>
    /// <param name="Span">Диапазон ссылки в исходной формуле.</param>
    public sealed record TreeLeaveReferenceFormulaExpression(
        Guid Uuid,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
