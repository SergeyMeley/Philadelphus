using Philadelphus.Core.Domain.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Relations;

/// <summary>
/// Задаёт контракт для вычисления связей объектов репозитория.
/// </summary>
public interface IRepositoryRelationsService
{
    /// <summary>
    /// Возвращает непосредственные исходящие и входящие связи указанного объекта.
    /// </summary>
    /// <param name="repository">Репозиторий, в пределах которого выполняется поиск.</param>
    /// <param name="source">Исходный объект.</param>
    /// <returns>Упорядоченная коллекция непосредственных связей.</returns>
    IReadOnlyList<RepositoryRelationModel> GetDirectRelations(
        PhiladelphusRepositoryModel repository,
        IMainEntityModel source);
}
