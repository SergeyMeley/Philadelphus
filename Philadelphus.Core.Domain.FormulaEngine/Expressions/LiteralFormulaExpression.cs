using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Литеральное значение формулы.
    /// </summary>
    /// <param name="Value">Значение литерала.</param>
    /// <param name="Type">Системный тип литерала.</param>
    /// <param name="Span">Диапазон литерала в исходной формуле.</param>
    public sealed record LiteralFormulaExpression(
        object? Value,
        SystemBaseType Type,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
