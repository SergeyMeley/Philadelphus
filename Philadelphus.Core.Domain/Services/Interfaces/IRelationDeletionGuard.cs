using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Проверяет наличие связей, блокирующих удаление объекта репозитория.
/// </summary>
public interface IRelationDeletionGuard
{
    /// <summary>
    /// Ищет атрибут, блокирующий удаление указанного объекта.
    /// </summary>
    /// <param name="target">Объект, возможность удаления которого необходимо проверить.</param>
    /// <param name="blockingAttribute">Найденный блокирующий атрибут либо null.</param>
    /// <returns>true, если блокирующий атрибут найден; иначе false.</returns>
    bool TryFindBlockingAttribute(IMainEntityModel target, out ElementAttributeModel? blockingAttribute);
}
