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
        /// Сохраняет текущую коллекцию значений как короткую формулу массива.
        /// </summary>
        public static void AssignValuesAsFormula(this ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            attribute.ValueFormula = FormulaReferenceFormatter.CreateTreeLeaveCollectionFormula(
                attribute.Values.Select(x => x.Uuid));
            attribute.ValueFormulaErrorCode = string.Empty;
        }

        /// <summary>
        /// Пытается назначить строковое значение системного базового типа через формулу-ссылку.
        /// </summary>
        public static bool TrySetSystemBaseValueAsFormula(
            this ElementAttributeModel attribute,
            string? value)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.TrySetSystemBaseValueFromString(value) == false)
            {
                return false;
            }

            // TrySet уже присвоил Value и выставил признак переопределения. Но очистка формулы
            // у УНАСЛЕДОВАННОГО атрибута сбрасывает _isValueOverridden (сеттер ValueFormula
            // сравнивает с формулой родителя: "" == "" → переопределение снимается), и эффективное
            // значение откатывалось к унаследованному — значение «обнулялось». Поэтому очищаем
            // формулу и ПЕРЕ-присваиваем значение последним действием, чтобы вернуть признак
            // переопределения (как в AssignValue, где значение ставится ПОСЛЕ очистки формулы).
            var assignedValue = attribute.Value;
            attribute.AssignValueAsFormula(assignedValue);
            return true;
        }

        /// <summary>
        /// Очищает формулу, ошибку её вычисления и материализованное значение атрибута.
        /// </summary>
        public static void ClearFormulaValue(this ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            attribute.ValueFormula = string.Empty;
            attribute.ValueFormulaErrorCode = string.Empty;
            if (attribute.IsCollectionValue)
            {
                attribute.ClearValuesCollection();
            }
            else
            {
                attribute.Value = null!;
            }
        }

        /// <summary>
        /// Разбирает введённый в ячейку текст и присваивает значение/формулу модели.
        /// Эквивалент сеттера <c>ElementAttributeVM.FormulaValueText</c>.
        /// </summary>
        public static void SetFormulaText(this ElementAttributeModel attribute, string? value)
        {
            ArgumentNullException.ThrowIfNull(attribute);

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

            if (attribute.IsCollectionValue)
            {
                return;
            }

            if (FormulaReferenceParser.TryParseTreeLeaveReference(trimmedValue, out var valueUuid)
                && attribute.ValuesList?.FirstOrDefault(x => x.Uuid == valueUuid) is TreeLeaveModel referencedValue)
            {
                attribute.AssignValueAsFormula(referencedValue);
                return;
            }

            attribute.TrySetSystemBaseValueAsFormula(trimmedValue);
        }

        /// <summary>
        /// Пытается преобразовать обычный текст значения в формулу-ссылку на допустимый лист атрибута.
        /// </summary>
        /// <param name="attribute">Атрибут, для которого разрешается значение.</param>
        /// <param name="text">Ссылка <c>[uuid]</c>, строковое системное значение или имя листа.</param>
        /// <param name="formula">Сформированная формула вида <c>=[uuid]</c>.</param>
        /// <returns><see langword="true"/>, если значение найдено или создано; иначе <see langword="false"/>.</returns>
        public static bool TryCreateValueFormulaFromText(
            this ElementAttributeModel attribute,
            string text,
            out string formula)
        {
            ArgumentNullException.ThrowIfNull(attribute);
            ArgumentNullException.ThrowIfNull(text);

            formula = string.Empty;
            TreeLeaveModel? resolvedValue = null;
            var trimmedText = text.Trim();

            if (FormulaReferenceParser.TryParseTreeLeaveReference(trimmedText, out var valueUuid)
                && attribute.ValuesList?.FirstOrDefault(x => x.Uuid == valueUuid) is TreeLeaveModel referencedValue)
            {
                resolvedValue = referencedValue;
            }
            else if (attribute.ValueType is SystemBaseTreeNodeModel)
            {
                if (attribute.TrySetSystemBaseValueFromString(text) == false)
                {
                    return false;
                }

                resolvedValue = attribute.Value;
            }
            else
            {
                resolvedValue = attribute.ValuesList?.FirstOrDefault(x =>
                    string.Equals(x.Name, text, StringComparison.Ordinal));
            }

            if (resolvedValue == null)
            {
                return false;
            }

            formula = FormulaReferenceFormatter.CreateTreeLeaveReferenceFormula(resolvedValue.Uuid);
            return true;
        }
    }
}
