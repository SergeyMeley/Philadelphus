using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.FormulaEngine.Formatting
{
    /// <summary>
    /// Содержит доменные правила использования ссылок в формулах.
    /// </summary>
    public static class FormulaReferenceRules
    {
        /// <summary>
        /// Проверяет, можно ли обратиться к атрибуту через относительную ссылку <c>АТРИБУТ(...)</c>.
        /// </summary>
        public static bool CanUseRelativeAttributeReference(
            ElementAttributeModel targetAttribute,
            ElementAttributeModel referencedAttribute)
        {
            ArgumentNullException.ThrowIfNull(targetAttribute);
            ArgumentNullException.ThrowIfNull(referencedAttribute);

            return targetAttribute.Owner?.Uuid == referencedAttribute.Owner?.Uuid;
        }
    }
}
