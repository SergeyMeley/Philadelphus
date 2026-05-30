using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Ссылка на лист дерева по UUID.
    /// </summary>
    public sealed record TreeLeaveReferenceFormulaExpression(
        Guid Uuid,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
