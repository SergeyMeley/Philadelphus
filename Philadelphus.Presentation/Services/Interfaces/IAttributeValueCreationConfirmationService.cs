using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Presentation.Services.Interfaces;

/// <summary>
/// Подтверждает включение созданного значения в редактируемый массив атрибута.
/// </summary>
public interface IAttributeValueCreationConfirmationService
{
    /// <summary>
    /// Запрашивает добавление созданного листа в массив с учётом сессионного решения.
    /// </summary>
    /// <param name="createdLeave">Созданный лист.</param>
    /// <param name="attribute">Редактируемый коллекционный атрибут.</param>
    /// <returns>true, если лист следует добавить в массив; иначе false.</returns>
    Task<bool> ConfirmAdditionAsync(
        TreeLeaveModel createdLeave,
        ElementAttributeModel attribute);
}
