using Philadelphus.Core.Domain.FormulaEngine.Contracts;

namespace Philadelphus.Core.Domain.FormulaEngine.Registry
{
    /// <summary>
    /// Реестр определений формул с поиском по имени или псевдониму.
    /// </summary>
    public sealed class FormulaRegistry
    {
        /// <summary>
        /// Индекс формул по нормализованному имени или псевдониму.
        /// </summary>
        private readonly Dictionary<string, FormulaDefinition> _formulasByKey = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Список формул в порядке регистрации.
        /// </summary>
        private readonly List<FormulaDefinition> _formulas = new();

        /// <summary>
        /// Зарегистрированные формулы.
        /// </summary>
        public IReadOnlyList<FormulaDefinition> Formulas => _formulas.AsReadOnly();

        /// <summary>
        /// Регистрирует формулу.
        /// </summary>
        /// <param name="formula">Определение формулы.</param>
        /// <exception cref="FormulaRegistrationException">Если имя или псевдоним уже заняты.</exception>
        public void Register(FormulaDefinition formula)
        {
            ArgumentNullException.ThrowIfNull(formula);

            var name = NormalizeKey(formula.Name, nameof(formula.Name));
            var keys = new[] { name }
                .Concat(formula.Aliases.Select(alias => NormalizeKey(alias, nameof(formula.Aliases))))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var key in keys)
            {
                if (_formulasByKey.TryGetValue(key, out var existing))
                {
                    throw new FormulaRegistrationException(
                        $"Ключ формулы '{key}' уже зарегистрирован для формулы '{existing.Name}'.");
                }
            }

            foreach (var key in keys)
            {
                _formulasByKey.Add(key, formula);
            }

            _formulas.Add(formula);
        }

        /// <summary>
        /// Регистрирует все формулы поставщика.
        /// </summary>
        /// <param name="provider">Поставщик формул.</param>
        public void RegisterProvider(IFormulaProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);

            foreach (var formula in provider.GetFormulas())
            {
                Register(formula);
            }
        }

        /// <summary>
        /// Пытается найти формулу по имени или псевдониму.
        /// </summary>
        /// <param name="nameOrAlias">Имя или псевдоним формулы.</param>
        /// <param name="formula">Найденное определение формулы.</param>
        /// <returns>true, если формула найдена; иначе false.</returns>
        public bool TryResolve(string nameOrAlias, out FormulaDefinition? formula)
        {
            var key = NormalizeKey(nameOrAlias, nameof(nameOrAlias));
            return _formulasByKey.TryGetValue(key, out formula);
        }

        /// <summary>
        /// Ищет формулу по имени или псевдониму и возвращает ошибку, если формула неизвестна.
        /// </summary>
        /// <param name="nameOrAlias">Имя или псевдоним формулы.</param>
        /// <returns>Результат поиска формулы.</returns>
        public FormulaResolveResult Resolve(string nameOrAlias)
        {
            var key = NormalizeKey(nameOrAlias, nameof(nameOrAlias));

            return _formulasByKey.TryGetValue(key, out var formula)
                ? FormulaResolveResult.Resolved(formula)
                : FormulaResolveResult.Unknown(key);
        }

        /// <summary>
        /// Нормализует ключ поиска формулы.
        /// </summary>
        /// <param name="value">Исходное имя или псевдоним формулы.</param>
        /// <param name="parameterName">Имя параметра для диагностик валидации.</param>
        /// <returns>Нормализованный ключ поиска.</returns>
        private static string NormalizeKey(string value, string parameterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

            return value.Trim();
        }
    }
}
