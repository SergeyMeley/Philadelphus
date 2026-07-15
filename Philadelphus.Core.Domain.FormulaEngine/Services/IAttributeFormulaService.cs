using System;
using System.Collections.Generic;

using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Execution;

namespace Philadelphus.Core.Domain.FormulaEngine.Services
{
    /// <summary>
    /// Доменная оркестрация пересчёта формул атрибутов: вычисление одной формулы с предварительным
    /// пересчётом зависимостей и детектом циклов, материализация результата в <c>Value</c> (кеш),
    /// определение ссылок одной формулы на другие атрибуты.
    /// </summary>
    /// <remarks>
    /// Вынесено из presentation (<c>RepositoryFormulaBarVM</c>), чтобы бизнес-логика формул жила в Core.
    /// Живёт в проекте FormulaEngine, т.к. использует и типы движка (контекст/результат/вычислитель),
    /// и доменные типы. Построение контекста выполнения и обновление UI остаются за вызывающим: контекст
    /// приходит через <paramref name="contextFactory" />, оповещение — через <paramref name="onAttributeChanged" />.
    /// </remarks>
    public interface IAttributeFormulaService
    {
        /// <summary>
        /// Рекурсивно пересчитывает формулу атрибута (сначала зависимости, затем сам атрибут),
        /// материализует результат в <c>Value</c> или выставляет код ошибки. Возвращает true, если
        /// атрибут не блокирует зависящие от него формулы (вычислен, либо ошибка сохранена без срыва графа).
        /// </summary>
        /// <param name="attribute">Пересчитываемый атрибут.</param>
        /// <param name="formulaOverride">Текст формулы вместо <c>attribute.ValueFormula</c> (например, при применении ввода).</param>
        /// <param name="stack">Текущий стек рекурсии (детект циклов).</param>
        /// <param name="recalculated">Уже пересчитанные в этом проходе атрибуты.</param>
        /// <param name="contextFactory">Фабрика контекста выполнения для атрибута (строит вызывающий слой).</param>
        /// <param name="onAttributeChanged">Колбэк оповещения об изменении атрибута (обновление UI).</param>
        bool RecalculateAttribute(
            ElementAttributeModel attribute,
            string? formulaOverride,
            ISet<Guid> stack,
            ISet<Guid> recalculated,
            Func<ElementAttributeModel, FormulaExecutionContext> contextFactory,
            Action<ElementAttributeModel> onAttributeChanged);

        /// <summary>
        /// Возвращает формульные атрибуты того же владельца, на которые ссылается переданная формула.
        /// </summary>
        IReadOnlyList<ElementAttributeModel> GetReferencedFormulaAttributes(
            ElementAttributeModel targetAttribute,
            string formulaText);

        /// <summary>
        /// Признак того, что формула ссылается на указанный атрибут (через <c>АТРИБУТ("имя")</c>).
        /// </summary>
        bool FormulaReferencesAttribute(string formula, ElementAttributeModel attribute);
    }
}
