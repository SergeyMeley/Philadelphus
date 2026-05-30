using Philadelphus.Core.Domain.FormulaEngine.Contracts;
using Philadelphus.Core.Domain.FormulaEngine.Errors;

namespace Philadelphus.Core.Domain.FormulaEngine.Registry
{
    /// <summary>
    /// Результат поиска формулы в реестре.
    /// </summary>
    public sealed class FormulaResolveResult
    {
        private FormulaResolveResult(FormulaDefinition? formula, FormulaError? error)
        {
            Formula = formula;
            Error = error;
        }

        /// <summary>
        /// Признак успешного поиска формулы.
        /// </summary>
        public bool IsResolved => Formula is not null;

        /// <summary>
        /// Найденное определение формулы.
        /// </summary>
        public FormulaDefinition? Formula { get; }

        /// <summary>
        /// Ошибка поиска, если формула не найдена.
        /// </summary>
        public FormulaError? Error { get; }

        /// <summary>
        /// Создает успешный результат поиска.
        /// </summary>
        /// <param name="formula">Найденная формула.</param>
        /// <returns>Успешный результат поиска.</returns>
        public static FormulaResolveResult Resolved(FormulaDefinition formula)
        {
            ArgumentNullException.ThrowIfNull(formula);

            return new FormulaResolveResult(formula, null);
        }

        /// <summary>
        /// Создает результат поиска для неизвестной формулы.
        /// </summary>
        /// <param name="nameOrAlias">Имя или псевдоним формулы.</param>
        /// <returns>Неуспешный результат поиска.</returns>
        public static FormulaResolveResult Unknown(string nameOrAlias)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameOrAlias);

            return new FormulaResolveResult(
                null,
                new FormulaError
                {
                    Code = FormulaErrorCode.UnknownFunction,
                    Message = $"Формула '{nameOrAlias}' не зарегистрирована.",
                    FunctionOrOperator = nameOrAlias
                });
        }
    }
}
