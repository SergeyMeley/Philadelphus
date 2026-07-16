using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;

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
                : FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(attribute.Value.Uuid);
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
                attribute.ClearFormulaValue();
                return;
            }

            var trimmedValue = value.Trim();
            if (trimmedValue.StartsWith("=", StringComparison.Ordinal))
            {
                attribute.ValueFormula = trimmedValue;
                attribute.ValueFormulaErrorCode = string.Empty;
                return;
            }

            if (FormulaReferenceParser.TryParseTreeLeaveReference(trimmedValue, out var valueUuid)
                && attribute.ValuesList?.FirstOrDefault(x => x.Uuid == valueUuid) is TreeLeaveModel referencedValue)
            {
                attribute.AssignValueAsFormula(referencedValue);
                return;
            }

            if (attribute.TrySetSystemBaseValueFromString(trimmedValue))
            {
                // TrySet уже присвоил Value и выставил признак переопределения. Но очистка формулы
                // ниже у УНАСЛЕДОВАННОГО атрибута сбрасывает _isValueOverridden (сеттер ValueFormula
                // сравнивает с формулой родителя: "" == "" → переопределение снимается), и эффективное
                // значение откатывалось к унаследованному — значение «обнулялось». Поэтому очищаем
                // формулу и ПЕРЕ-присваиваем значение последним действием, чтобы вернуть признак
                // переопределения (как в AssignValue, где значение ставится ПОСЛЕ очистки формулы).
                var assignedValue = attribute.Value;
                attribute.AssignValueAsFormula(assignedValue);
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

    }
}
