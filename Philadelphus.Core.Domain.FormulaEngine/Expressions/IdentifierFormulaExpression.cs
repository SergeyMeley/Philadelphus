using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Expressions
{
    /// <summary>
    /// Идентификатор формулы.
    /// </summary>
    /// <param name="Name">Текст идентификатора.</param>
    /// <param name="Span">Диапазон идентификатора в исходной формуле.</param>
    public sealed record IdentifierFormulaExpression(
        string Name,
        FormulaTextSpan Span) : FormulaExpression(Span);
}
