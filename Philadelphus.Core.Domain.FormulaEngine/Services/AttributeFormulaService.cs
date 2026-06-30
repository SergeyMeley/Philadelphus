using System;
using System.Collections.Generic;
using System.Linq;

using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool FormulaReferencesAttribute(string formula, ElementAttributeModel attribute)
        {
            return formula.Contains(CreateRelativeAttributeReference(attribute), StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public string CreateRelativeAttributeReference(ElementAttributeModel attribute)
        {
            var escapedName = (attribute.Name ?? string.Empty).Replace("\"", "\"\"", StringComparison.Ordinal);
            return $"АТРИБУТ(\"{escapedName}\")";
        }

        private bool TryApplyFormulaResult(ElementAttributeModel targetAttribute, FormulaResult result, string formulaText)
        {
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
