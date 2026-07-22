using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;

namespace Philadelphus.Presentation.Services.Tables
{
    /// <summary>
    /// Единая логика текстового представления значения атрибута для UI: «отображаемый текст»
    /// (результат формулы / код ошибки / человекочитаемое значение) и «текст редактирования»
    /// (формула или ссылка «[uuid]»).
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

            if (attribute.IsCollectionValue)
            {
                return string.Join("; ", attribute.Values.Select(x => x.Name));
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

            if (attribute.IsCollectionValue)
            {
                return FormulaReferenceFormatter.CreateTreeLeaveCollectionFormula(
                    attribute.Values.Select(x => x.Uuid));
            }

            return attribute.Value?.Uuid == null
                ? string.Empty
                : FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(attribute.Value.Uuid);
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

    }
}
