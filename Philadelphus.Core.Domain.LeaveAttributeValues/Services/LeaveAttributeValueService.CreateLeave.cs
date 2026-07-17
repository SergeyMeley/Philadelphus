using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.LeaveAttributeValues.Signatures;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

public sealed partial class LeaveAttributeValueService : ILeaveAttributeValueService
{
    private readonly IPhiladelphusRepositoryService _repositoryService;

    /// <summary>
    /// Инициализирует сервис операций со значениями атрибутов листов.
    /// </summary>
    /// <param name="repositoryService">Доменный сервис создания моделей репозитория.</param>
    public LeaveAttributeValueService(
        IPhiladelphusRepositoryService repositoryService)
    {
        ArgumentNullException.ThrowIfNull(repositoryService);

        _repositoryService = repositoryService;
    }

    /// <summary>
    /// Создаёт лист с автоименем и заполняет переданные значения атрибутов
    /// без немедленного сохранения в репозитории.
    /// </summary>
    /// <param name="parentNode">Родительский узел нового листа.</param>
    /// <param name="sourceAttributes">Исходные значения атрибутов.</param>
    /// <returns>Созданный лист в состоянии <c>Initialized</c>.</returns>
    public TreeLeaveModel CreateLeave(
        TreeNodeModel parentNode,
        IEnumerable<ElementAttributeModel> sourceAttributes)
    {
        ArgumentNullException.ThrowIfNull(parentNode);
        ArgumentNullException.ThrowIfNull(sourceAttributes);

        var sourceAttributeList = sourceAttributes.ToList();
        ValidateSourceAttributes(parentNode, sourceAttributeList);

        var createdLeave = _repositoryService.CreateTreeLeave(
            parentNode,
            needAutoName: true,
            withoutInfoNotifications: true)
            ?? throw new InvalidOperationException(
                $"Не удалось создать лист в узле '{parentNode.Name}' [{parentNode.Uuid}].");

        var declaringUuids = sourceAttributeList
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
        FillAttributes(createdLeave, sourceAttributeList, declaringUuids);
        return createdLeave;
    }

    /// <summary>
    /// Проверяет значения до создания модели, чтобы не оставлять частично
    /// заполненный лист при предсказуемой ошибке входных данных.
    /// </summary>
    private static void ValidateSourceAttributes(
        TreeNodeModel parentNode,
        IReadOnlyCollection<ElementAttributeModel> sourceAttributes)
    {
        if (LeaveAttributeValueSignature.Create(sourceAttributes).IsValid == false)
            throw new InvalidOperationException("Невозможно создать лист: значения атрибутов не разрешены.");

        var parentAttributes = parentNode.Attributes.ToDictionary(x => x.DeclaringUuid);
        foreach (var sourceAttribute in sourceAttributes)
        {
            if (parentAttributes.TryGetValue(sourceAttribute.DeclaringUuid, out var parentAttribute) == false)
            {
                throw new InvalidOperationException(
                    $"Атрибут '{sourceAttribute.Name}' [{sourceAttribute.LocalUuid}] "
                    + $"не объявлен в узле '{parentNode.Name}' [{parentNode.Uuid}].");
            }

            EnsureCompatibleSource(parentAttribute, sourceAttribute);
        }
    }
}
