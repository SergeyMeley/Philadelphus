using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Литеральное значение формулы.
    /// </summary>
    public sealed record LiteralFormulaExpression(
        object? Value,
        SystemBaseType Type,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
