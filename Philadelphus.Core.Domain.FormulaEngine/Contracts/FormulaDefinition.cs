using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.Contracts
{
    /// <summary>
    /// Описание формулы: metadata и делегат вычисления.
    /// </summary>
    public sealed class FormulaDefinition
    {
        /// <summary>
        /// Основное имя формулы.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Дополнительные имена и операторные псевдонимы формулы.
        /// </summary>
        public IReadOnlyCollection<string> Aliases { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Человекочитаемое описание формулы.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// Описание аргументов формулы.
        /// </summary>
        public IReadOnlyList<FormulaArgumentDefinition> Arguments { get; init; } = Array.Empty<FormulaArgumentDefinition>();

        /// <summary>
        /// Делегат, выполняющий вычисление формулы.
        /// </summary>
        public required FormulaEvaluator Evaluator { get; init; }
    }
}
