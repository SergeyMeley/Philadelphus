using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Presentation.Services.Tables
{
    /// <summary>
    /// Единая логика текстового представления значения атрибута для UI: «отображаемый текст»
    /// (результат формулы / код ошибки / человекочитаемое значение) и «текст редактирования»
    /// (формула или ссылка «[uuid]»), а также разбор введённого текста обратно в модель.
    /// </summary>
    /// <remarks>
    /// Вынесено из <c>ElementAttributeVM</c>, чтобы ячейка значения в таблице атрибутов и в таблице
    /// наследников работала ПОЛНОСТЬЮ идентично (одна и та же логика над <see cref="ElementAttributeModel"/>).
    /// Касается только одиночного значения (не коллекции).
    /// </remarks>
    public static class AttributeValueText
    {
        /// <summary>
        /// Текст ячейки в режиме просмотра: код ошибки формулы либо человекочитаемое значение.
        /// Эквивалент <c>ElementAttributeVM.DisplayedValueText</c>.
        /// </summary>
        public static string GetDisplayText(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (string.IsNullOrWhiteSpace(attribute.ValueFormula) == false
                && string.IsNullOrWhiteSpace(attribute.ValueFormulaErrorCode) == false)
            {
                return attribute.ValueFormulaErrorCode;
            }

            return GetAssignedValueText(attribute);
        }

        /// <summary>
        /// Текст ячейки в режиме редактирования: формула или ссылка на лист «=[uuid]».
        /// Эквивалент геттера <c>ElementAttributeVM.FormulaValueText</c>.
        /// </summary>
        public static string GetFormulaText(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (string.IsNullOrWhiteSpace(attribute.ValueFormula) == false)
            {
                return attribute.ValueFormula;
            }

            return attribute.Value?.Uuid == null
                ? string.Empty
                : $"=[{attribute.Value.Uuid}]";
        }

        /// <summary>
        /// Разбирает введённый в ячейку текст и присваивает значение/формулу модели.
        /// Эквивалент сеттера <c>ElementAttributeVM.FormulaValueText</c>.
        /// </summary>
        public static void SetFormulaText(ElementAttributeModel attribute, string? value)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.IsCollectionValue)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                ClearValue(attribute);
                return;
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.StartsWith("=", StringComparison.Ordinal))
            {
                attribute.ValueFormula = trimmedValue;
                attribute.ValueFormulaErrorCode = string.Empty;
                return;
            }

            if (TryGetLeafUuidReference(trimmedValue, out var valueUuid)
                && attribute.ValuesList?.FirstOrDefault(x => x.Uuid == valueUuid) is TreeLeaveModel referencedValue)
            {
                AssignValue(attribute, referencedValue);
                return;
            }

            if (attribute.TrySetSystemBaseValueFromString(trimmedValue))
            {
                attribute.ValueFormula = string.Empty;
                attribute.ValueFormulaErrorCode = string.Empty;
            }
        }

        /// <summary>
        /// Человекочитаемое одиночное значение: для системных базовых типов — строковое значение,
        /// иначе — имя листа. Эквивалент <c>ElementAttributeVM.AssignedValueText</c> (геттер).
        /// </summary>
        private static string GetAssignedValueText(ElementAttributeModel attribute)
        {
            return attribute.Value is SystemBaseTreeLeaveModel systemBaseValue
                ? systemBaseValue.StringValue
                : attribute.Value?.Name ?? string.Empty;
        }

        private static void AssignValue(ElementAttributeModel attribute, TreeLeaveModel value)
        {
            attribute.ValueFormula = string.Empty;
            attribute.ValueFormulaErrorCode = string.Empty;
            attribute.Value = value;
        }

        private static void ClearValue(ElementAttributeModel attribute)
        {
            attribute.ValueFormula = string.Empty;
            attribute.ValueFormulaErrorCode = string.Empty;
            attribute.Value = null!;
        }

        private static bool TryGetLeafUuidReference(string text, out Guid uuid)
        {
            uuid = Guid.Empty;

            return text.Length == 38
                && text.StartsWith("[", StringComparison.Ordinal)
                && text.EndsWith("]", StringComparison.Ordinal)
                && Guid.TryParse(text[1..^1], out uuid);
        }
    }
}
