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
    }
}
