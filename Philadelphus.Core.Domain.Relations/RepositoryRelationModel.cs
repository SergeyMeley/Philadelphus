using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Relations;

/// <summary>
/// Описывает вычисленную непосредственную связь с объектом репозитория.
/// </summary>
/// <param name="Target">Связанный объект.</param>
/// <param name="Type">Тип связи.</param>
/// <param name="BlocksSourceDeletion">Признак блокировки удаления исходного объекта.</param>
/// <param name="NavigationOwnerUuid">UUID владельца, используемый для перехода к атрибуту.</param>
public sealed record RepositoryRelationModel(
    IMainEntityModel Target, 
    RepositoryRelationType Type,
    bool BlocksSourceDeletion, 
    Guid? NavigationOwnerUuid = null)
{
    /// <summary>
    /// Текстовое представление связанного объекта.
    /// </summary>
    public string DisplayName => $"'{Target.Name}' [{Target.Uuid}]";

    /// <summary>
    /// Локализованное наименование типа связи.
    /// </summary>
    public string TypeDisplayName => Type.GetDisplayName();
}
