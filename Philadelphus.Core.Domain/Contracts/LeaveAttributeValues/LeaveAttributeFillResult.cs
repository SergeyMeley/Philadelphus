using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;

/// <summary>
/// Результат заполнения атрибутов листа значениями другого листа.
/// </summary>
/// <param name="ChangedAttributes">Атрибуты целевого листа, значения которых изменились.</param>
public sealed record LeaveAttributeFillResult(
    IReadOnlyList<ElementAttributeModel> ChangedAttributes);
