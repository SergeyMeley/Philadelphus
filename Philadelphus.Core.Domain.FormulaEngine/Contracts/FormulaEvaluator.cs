using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Вычисляет формулу по уже вычисленным аргументам.
    /// </summary>
    public delegate FormulaResult FormulaEvaluator(
        FormulaExecutionContext context,
        IReadOnlyList<FormulaResult> arguments);
}
