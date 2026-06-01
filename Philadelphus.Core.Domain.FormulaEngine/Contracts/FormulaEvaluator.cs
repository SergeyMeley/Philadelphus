using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Вычисляет формулу по уже вычисленным аргументам.
    /// </summary>
    /// <param name="context">Контекст вычисления формулы.</param>
    /// <param name="arguments">Уже вычисленные аргументы формулы.</param>
    /// <returns>Результат вычисления формулы.</returns>
    public delegate FormulaResult FormulaEvaluator(
        FormulaExecutionContext context,
        IReadOnlyList<FormulaResult> arguments);
}
