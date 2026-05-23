using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

namespace Philadelphus.Core.Domain.Helpers
{
    /// <summary>
    /// Проверяет совместимость значения системного листа с системным типом атрибута.
    /// </summary>
    /// <remarks>
    /// Системный лист уже валидирует значение относительно собственного <see cref="SystemBaseTreeLeaveModel.SystemBaseType" />.
    /// Атрибут может ожидать другой системный тип, поэтому при присваивании одиночного или коллекционного
    /// значения нужна дополнительная проверка относительно <see cref="SystemBaseTreeNodeModel.SystemBaseType" /> атрибута.
    /// </remarks>
    internal static class SystemBaseAttributeValueCompatibilityValidator
    {
        /// <summary>
        /// Проверяет, совместимо ли значение с типом атрибута.
        /// </summary>
        /// <param name="valueType">Тип значения атрибута.</param>
        /// <param name="value">Проверяемое значение атрибута.</param>
        /// <param name="systemBaseType">Системный тип атрибута, если проверка применима.</param>
        /// <param name="stringValue">Строковое значение системного листа, если проверка применима.</param>
        /// <param name="expectedFormat">Описание ожидаемого формата, если значение несовместимо.</param>
        /// <returns>true, если значение совместимо или правило неприменимо; иначе false.</returns>
        public static bool IsCompatible(
            TreeNodeModel? valueType,
            TreeLeaveModel? value,
            out SystemBaseType? systemBaseType,
            out string? stringValue,
            out string expectedFormat)
        {
            systemBaseType = null;
            stringValue = null;
            expectedFormat = string.Empty;

            if (valueType is not SystemBaseTreeNodeModel systemBaseNode
                || value is not SystemBaseTreeLeaveModel systemBaseValue)
            {
                return true;
            }

            systemBaseType = systemBaseNode.SystemBaseType;
            stringValue = systemBaseValue.StringValue;
            return SystemBaseStringValueValidator.IsValid(systemBaseNode.SystemBaseType, stringValue, out expectedFormat);
        }
    }
}
