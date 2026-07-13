using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Relations;

/// <summary>
/// Вычисляет непосредственные структурные и предметные связи объектов репозитория.
/// </summary>
public sealed class RepositoryRelationsService : IRelationDeletionGuard
{
    /// <summary>
    /// Возвращает непосредственные исходящие и входящие связи указанного объекта.
    /// </summary>
    /// <param name="repository">Репозиторий, в пределах которого выполняется поиск.</param>
    /// <param name="source">Исходный объект.</param>
    /// <returns>Упорядоченная коллекция непосредственных связей.</returns>
    public IReadOnlyList<RepositoryRelationModel> GetDirectRelations(
        PhiladelphusRepositoryModel repository,
        IMainEntityModel source)
    {
        var result = new List<RepositoryRelationModel>();

        if (source is IChildrenModel child
            && child.Parent is IMainEntityModel parent)
            result.Add(new(parent, RepositoryRelationType.Parent, false));
        if (source is IParentModel parentModel)
            result.AddRange(parentModel.Childs.Values.OfType<IMainEntityModel>()
                .Select(x => new RepositoryRelationModel(x, RepositoryRelationType.Child, false)));
        if (source is IContentModel content
            && content.Owner is IMainEntityModel ownerEntity)
            result.Add(new(ownerEntity, RepositoryRelationType.Owner, false));
        if (source is IOwnerModel owner)
            result.AddRange(owner.Content.Values.OfType<IMainEntityModel>()
                .Select(x => new RepositoryRelationModel(x, RepositoryRelationType.Content, false, source.Uuid)));

        if (source is ElementAttributeModel attribute)
        {
            if (attribute.ValueType != null) 
                result.Add(new(attribute.ValueType, RepositoryRelationType.AttributeDataType, false));
            if (attribute.Value != null) 
                result.Add(new(attribute.Value, RepositoryRelationType.AttributeValue, false));
            if (attribute.Values != null && attribute.Values.Any())
                result.AddRange(attribute.Values.Select(x => new RepositoryRelationModel(x, RepositoryRelationType.AttributeCollectionValue, false)));

            foreach (var uuid in FormulaReferenceExtractor.GetTreeLeaveUuids(attribute.ValueFormula))
                if (repository.AllContentRecursive.Values.OfType<IMainEntityModel>().FirstOrDefault(x => x.Uuid == uuid) is { } target)
                    result.Add(new(target, RepositoryRelationType.FormulaReference, false));
        }

        foreach (var candidate in repository.AllContentRecursive.Values.OfType<ElementAttributeModel>())
            AddIncomingAttributeRelations(result, source, candidate);

        return result
            .DistinctBy(x => (x.Target.Uuid, x.Type, x.BlocksSourceDeletion))
            .OrderByDescending(x => x.BlocksSourceDeletion)
            .ThenBy(x => x.Type)
            .ThenBy(x => x.Target.Name)
            .ThenBy(x => x.Target.Uuid)
            .ToList();
    }

    /// <summary>
    /// Ищет атрибут, блокирующий удаление указанного объекта.
    /// </summary>
    /// <param name="target">Объект, возможность удаления которого необходимо проверить.</param>
    /// <param name="blockingAttribute">Найденный блокирующий атрибут либо null.</param>
    /// <returns>true, если блокирующий атрибут найден; иначе false.</returns>
    public bool TryFindBlockingAttribute(
        IMainEntityModel target,
        out ElementAttributeModel? blockingAttribute)
    {
        var repository = target as PhiladelphusRepositoryModel
            ?? (target as IContentModel)?.AllOwnersRecursive.Values.OfType<PhiladelphusRepositoryModel>().FirstOrDefault();
        blockingAttribute = repository?.AllContentRecursive.Values.OfType<ElementAttributeModel>()
            .FirstOrDefault(x => IsBlockingReference(x, target));
        return blockingAttribute != null;
    }

    /// <summary>
    /// Добавляет входящие атрибутные связи с указанным объектом.
    /// </summary>
    /// <param name="result">Коллекция формируемых связей.</param>
    /// <param name="source">Объект, для которого вычисляются связи.</param>
    /// <param name="candidate">Проверяемый атрибут.</param>
    private static void AddIncomingAttributeRelations(
        ICollection<RepositoryRelationModel> result,
        IMainEntityModel source,
        ElementAttributeModel candidate)
    {
        var ownerUuid = (candidate.Owner as IMainEntityModel)?.Uuid;
        if (candidate.ValueType?.Uuid == source.Uuid)
            result.Add(new(candidate, RepositoryRelationType.AttributeDataType, true, ownerUuid));
        if (candidate.Value?.Uuid == source.Uuid)
            result.Add(new(candidate, RepositoryRelationType.AttributeValue, true, ownerUuid));
        if (candidate.Values.Any(x => x.Uuid == source.Uuid))
            result.Add(new(candidate, RepositoryRelationType.AttributeCollectionValue, true, ownerUuid));
        if (FormulaReferenceExtractor.GetTreeLeaveUuids(candidate.ValueFormula).Contains(source.Uuid))
            result.Add(new(candidate, RepositoryRelationType.FormulaReference, true, ownerUuid));
    }

    /// <summary>
    /// Проверяет наличие блокирующей атрибутной ссылки.
    /// </summary>
    /// <param name="attribute">Проверяемый атрибут.</param>
    /// <param name="target">Объект, возможность удаления которого проверяется.</param>
    /// <returns>true, если атрибут блокирует удаление объекта; иначе false.</returns>
    private static bool IsBlockingReference(
        ElementAttributeModel attribute, 
        IMainEntityModel target)
    {
        return attribute.Uuid != target.Uuid
            && (attribute.ValueType?.Uuid == target.Uuid
            || attribute.Value?.Uuid == target.Uuid
            || attribute.Values.Any(x => x.Uuid == target.Uuid)
            || FormulaReferenceExtractor.GetTreeLeaveUuids(attribute.ValueFormula).Contains(target.Uuid));
    }
        
}
