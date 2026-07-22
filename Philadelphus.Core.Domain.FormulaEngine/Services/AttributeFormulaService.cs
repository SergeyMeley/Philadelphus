using System;
using System.Collections.Generic;
using System.Linq;

using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Errors;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.FormulaEngine.Services
{
    /// <summary>
    /// Доменная реализация оркестрации пересчёта формул атрибутов. Перенесена из presentation
    /// (<c>RepositoryFormulaBarVM</c>) без изменения логики; UI-оповещение и построение контекста
    /// делегируются вызывающему через колбэк/фабрику.
    /// </summary>
    public sealed class AttributeFormulaService : IAttributeFormulaService
    {
        private readonly FormulaAstEvaluator _formulaEvaluator;
        private readonly INotificationService _notificationService;

        public AttributeFormulaService(
            FormulaAstEvaluator formulaEvaluator,
            INotificationService notificationService)
        {
            ArgumentNullException.ThrowIfNull(formulaEvaluator);
            ArgumentNullException.ThrowIfNull(notificationService);

            _formulaEvaluator = formulaEvaluator;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Рекурсивно пересчитывает формулу атрибута вместе с формулами, от которых она зависит.
        /// </summary>
        /// <param name="attribute">Пересчитываемый атрибут.</param>
        /// <param name="formulaOverride">Формула, временно используемая вместо сохраненной формулы атрибута.</param>
        /// <param name="stack">Стек текущего рекурсивного обхода для обнаружения циклических зависимостей.</param>
        /// <param name="recalculated">UUID уже пересчитанных в текущем проходе атрибутов.</param>
        /// <param name="contextFactory">Фабрика контекста вычисления для конкретного атрибута.</param>
        /// <param name="onAttributeChanged">Обработчик обновления представления изменившегося атрибута.</param>
        /// <returns>
        /// <see langword="true" />, если атрибут не блокирует дальнейший пересчет зависимых формул;
        /// иначе <see langword="false" />.
        /// </returns>
        public bool RecalculateAttribute(
            ElementAttributeModel attribute,
            string? formulaOverride,
            ISet<Guid> stack,
            ISet<Guid> recalculated,
            Func<ElementAttributeModel, FormulaExecutionContext> contextFactory,
            Action<ElementAttributeModel> onAttributeChanged)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            ArgumentNullException.ThrowIfNull(stack);
            ArgumentNullException.ThrowIfNull(recalculated);
            ArgumentNullException.ThrowIfNull(contextFactory);
            ArgumentNullException.ThrowIfNull(onAttributeChanged);

            var formulaText = formulaOverride ?? attribute.ValueFormula;
            if (string.IsNullOrWhiteSpace(formulaText))
            {
                return true;
            }

            if (recalculated.Contains(attribute.Uuid))
            {
                return true;
            }

            if (stack.Add(attribute.Uuid) == false)
            {
                // Вычисленное Value — лишь материализованный результат. При ошибке старый результат
                // необходимо удалить, иначе в БД и отчетах останется значение от предыдущей формулы.
                ClearMaterializedFormulaResult(attribute);
                attribute.ValueFormula = formulaText;
                attribute.ValueFormulaErrorCode = "#CYCLE!";
                onAttributeChanged(attribute);
                _notificationService.SendTextMessage<AttributeFormulaService>(
                    $"Обнаружена циклическая зависимость формулы атрибута '{attribute.Name}'.",
                    NotificationCriticalLevelModel.Warning);
                return false;
            }

            // Сначала пересчитываем формулы, на которые ссылается текущая формула,
            // чтобы результат не зависел от устаревших вычисленных значений.
            foreach (var dependency in GetReferencedFormulaAttributes(attribute, formulaText))
            {
                if (RecalculateAttribute(dependency, null, stack, recalculated, contextFactory, onAttributeChanged) == false)
                {
                    ClearMaterializedFormulaResult(attribute);
                    attribute.ValueFormula = formulaText;
                    attribute.ValueFormulaErrorCode = "#DEPENDENCY!";
                    onAttributeChanged(attribute);
                    stack.Remove(attribute.Uuid);
                    return true;
                }
            }

            var result = _formulaEvaluator.Evaluate(formulaText, contextFactory(attribute));
            if (result.IsSuccess == false)
            {
                ClearMaterializedFormulaResult(attribute);
                attribute.ValueFormula = formulaText;
                attribute.ValueFormulaErrorCode = FormatFormulaErrorCode(result);
                onAttributeChanged(attribute);
                _notificationService.SendTextMessage<AttributeFormulaService>(
                    $"Формула сохранена, но не вычислена: {result.Error?.Message}",
                    NotificationCriticalLevelModel.Warning);
                stack.Remove(attribute.Uuid);
                recalculated.Add(attribute.Uuid);
                return true;
            }

            var isApplied = TryApplyFormulaResult(attribute, result, formulaText);
            if (isApplied)
            {
                onAttributeChanged(attribute);
                recalculated.Add(attribute.Uuid);
            }

            stack.Remove(attribute.Uuid);
            return isApplied;
        }

        /// <summary>
        /// Возвращает формульные атрибуты того же владельца, на которые ссылается указанная формула.
        /// </summary>
        /// <param name="targetAttribute">Атрибут, для которого анализируется формула.</param>
        /// <param name="formulaText">Текст анализируемой формулы.</param>
        /// <returns>Список найденных зависимостей без повторяющихся UUID.</returns>
        public IReadOnlyList<ElementAttributeModel> GetReferencedFormulaAttributes(
            ElementAttributeModel targetAttribute,
            string formulaText)
        {
            if (targetAttribute.Owner is not IAttributeOwnerModel owner)
            {
                return Array.Empty<ElementAttributeModel>();
            }

            return owner.Attributes
                .Where(x => x.Uuid != targetAttribute.Uuid
                    && string.IsNullOrWhiteSpace(x.ValueFormula) == false
                    && FormulaReferencesAttribute(formulaText, x))
                .GroupBy(x => x.Uuid)
                .Select(x => x.First())
                .ToList();
        }

        /// <summary>
        /// Проверяет наличие относительной ссылки на указанный атрибут в тексте формулы.
        /// </summary>
        /// <param name="formula">Текст проверяемой формулы.</param>
        /// <param name="attribute">Атрибут, ссылку на который необходимо найти.</param>
        /// <returns><see langword="true" />, если ссылка найдена; иначе <see langword="false" />.</returns>
        public bool FormulaReferencesAttribute(string formula, ElementAttributeModel attribute)
        {
            var reference = FormulaReferenceFormatter.CreateRelativeAttributeReference(attribute.Name);
            return formula.Contains(reference, StringComparison.OrdinalIgnoreCase);
        }

        private bool TryApplyFormulaResult(ElementAttributeModel targetAttribute, FormulaResult result, string formulaText)
        {
            if (targetAttribute.IsCollectionValue)
            {
                if (result.TreeLeaves == null
                    || result.TreeLeaves.Any(x => IsTreeLeaveCompatible(targetAttribute, x) == false))
                {
                    ClearMaterializedFormulaResult(targetAttribute);
                    targetAttribute.ValueFormula = formulaText;
                    targetAttribute.ValueFormulaErrorCode = $"#{FormulaErrorCode.TypeMismatch}!";
                    SendFormulaTypeMismatch(targetAttribute, result.ValueType.ToString());
                    return true;
                }

                if (targetAttribute.TrySetValuesFromFormula(result.TreeLeaves) == false)
                    return false;

                targetAttribute.ValueFormula = formulaText;
                targetAttribute.ValueFormulaErrorCode = string.Empty;
                return true;
            }

            if (result.TreeLeave != null)
            {
                if (IsTreeLeaveCompatible(targetAttribute, result.TreeLeave) == false)
                {
                    SendFormulaTypeMismatch(targetAttribute, result.TreeLeave.ParentNode.Name);
                    return false;
                }

                targetAttribute.Value = result.TreeLeave;
                targetAttribute.ValueFormula = formulaText;
                targetAttribute.ValueFormulaErrorCode = string.Empty;
                return true;
            }

            if (targetAttribute.ValueType is not SystemBaseTreeNodeModel systemBaseNode)
            {
                SendFormulaTypeMismatch(targetAttribute, result.ValueType.ToString());
                return false;
            }

            if (IsSystemBaseResultCompatible(systemBaseNode.SystemBaseType, result.ValueType) == false
                || SystemBaseStringValueValidator.TryFormat(systemBaseNode.SystemBaseType, result.Value, out var stringValue) == false)
            {
                SendFormulaTypeMismatch(targetAttribute, result.ValueType.ToString());
                return false;
            }

            if (targetAttribute.TrySetSystemBaseValueFromString(stringValue) == false)
            {
                return false;
            }

            targetAttribute.ValueFormula = formulaText;
            targetAttribute.ValueFormulaErrorCode = string.Empty;
            return true;
        }

        private static bool IsTreeLeaveCompatible(ElementAttributeModel attribute, TreeLeaveModel value)
        {
            return attribute.ValueType?.Uuid == value.ParentNode.Uuid;
        }

        private static void ClearMaterializedFormulaResult(ElementAttributeModel attribute)
        {
            if (attribute.IsCollectionValue)
            {
                attribute.ClearValuesFromFormula();
            }
            else
            {
                attribute.Value = null!;
            }
        }

        private static bool IsSystemBaseResultCompatible(SystemBaseType expectedType, SystemBaseType actualType)
        {
            return expectedType == actualType
                || expectedType == SystemBaseType.OBJECT
                || expectedType == SystemBaseType.NUMERIC && actualType is SystemBaseType.INTEGER or SystemBaseType.FLOAT;
        }

        private void SendFormulaTypeMismatch(ElementAttributeModel attribute, string actualType)
        {
            _notificationService.SendTextMessage<AttributeFormulaService>(
                $"Тип результата формулы '{actualType}' не соответствует типу данных атрибута '{attribute.Name}' ({attribute.ValueType?.Name}).",
                NotificationCriticalLevelModel.Warning);
        }

        private static string FormatFormulaErrorCode(FormulaResult result)
        {
            return result.Error == null
                ? "#ERROR!"
                : $"#{result.Error.Code}!";
        }
    }
}
