using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;

namespace Philadelphus.Core.Domain.FormulaEngine.Extensions
{
    /// <summary>
    /// Содержит доменные операции изменения формульного значения атрибута.
    /// </summary>
    public static class ElementAttributeFormulaExtensions
    {
        /// <summary>
        /// Назначает выбранный лист через формулу-ссылку. <c>Value</c> заполняется только как материализованный
        /// runtime-результат; при следующей загрузке он будет проигнорирован и вычислен заново.
        /// </summary>
        public static void AssignValueAsFormula(this ElementAttributeModel attribute, TreeLeaveModel value)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            ArgumentNullException.ThrowIfNull(value);

            attribute.ValueFormula = FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(value.Uuid);
            attribute.ValueFormulaErrorCode = string.Empty;
            attribute.Value = value;
        }

        /// <summary>
        /// Очищает формулу, ошибку её вычисления и материализованное значение атрибута.
        /// </summary>
        public static void ClearFormulaValue(this ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            attribute.ValueFormula = string.Empty;
            attribute.ValueFormulaErrorCode = string.Empty;
            attribute.Value = null!;
        }

        /// <summary>
        /// Разбирает введённый в ячейку текст и присваивает значение/формулу модели.
        /// Эквивалент сеттера <c>ElementAttributeVM.FormulaValueText</c>.
        /// </summary>
        public static void SetFormulaText(this ElementAttributeModel attribute, string? value)
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
    }
}
