using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Выполняет переиспользуемые операции со значениями атрибутов листов.
/// </summary>
public interface ILeaveAttributeValueService
{
    /// <summary>
    /// Находит листы с такими же значениями явно переданного набора атрибутов.
    /// </summary>
    /// <param name="expectedAttributes">Ожидаемые атрибуты, сопоставляемые по <c>DeclaringUuid</c>.</param>
    /// <param name="candidates">Листы, среди которых выполняется поиск.</param>
    /// <returns>Признак валидности сравнения и найденные листы.</returns>
    LeaveAttributeMatchResult FindMatches(
        IEnumerable<ElementAttributeModel> expectedAttributes,
        IEnumerable<TreeLeaveModel> candidates);

    /// <summary>
    /// Заполняет выбранные атрибуты целевого элемента значениями исходного листа.
    /// </summary>
    /// <param name="targetOwner">Заполняемый узел или лист.</param>
    /// <param name="sourceLeave">Лист-источник.</param>
    /// <param name="declaringUuids">Идентификаторы объявлений заполняемых атрибутов.</param>
    /// <returns>Сведения об изменённых атрибутах.</returns>
    LeaveAttributeFillResult FillFromLeave(
        IAttributeOwnerModel targetOwner,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids);

    /// <summary>
    /// Без изменения моделей рассчитывает число атрибутов, которые будут перезаписаны
    /// операцией <see cref="FillFromLeave" />.
    /// </summary>
    /// <param name="targetOwner">Проверяемый узел или лист.</param>
    /// <param name="sourceLeave">Лист-источник.</param>
    /// <param name="declaringUuids">Идентификаторы объявлений сравниваемых атрибутов.</param>
    /// <returns>Точное количество изменяемых атрибутов.</returns>
    int CountFillChanges(
        IAttributeOwnerModel targetOwner,
        TreeLeaveModel sourceLeave,
        IEnumerable<Guid> declaringUuids);

    /// <summary>
    /// Создаёт лист и переносит в него переданные значения атрибутов.
    /// </summary>
    /// <param name="parentNode">Узел создаваемого листа.</param>
    /// <param name="sourceAttributes">Атрибуты-источники значений.</param>
    /// <returns>Созданный без немедленного сохранения лист.</returns>
    TreeLeaveModel CreateLeave(
        TreeNodeModel parentNode,
        IEnumerable<ElementAttributeModel> sourceAttributes);
}
