using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Mapping;

/// <summary>
/// Разрешает временные корреляции Excel FK и заполняет полиморфные связи
/// импортированных листов в порядке от родителей к потомкам.
/// </summary>
internal sealed class ImportLeavePolymorphismLinker
{
    private readonly ILeavePolymorphismService _service;

    /// <summary>
    /// Инициализирует обработчик корреляций доменным сервисом полиморфизма.
    /// </summary>
    /// <param name="service">Сервис заполнения и разрешения связей листов.</param>
    public ImportLeavePolymorphismLinker(ILeavePolymorphismService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    /// <summary>
    /// Заполняет импортированные листья по найденным родителям и сообщает прогресс.
    /// </summary>
    /// <param name="leavesByCorrelationId">Листы по идентификаторам исходных строк.</param>
    /// <param name="parentCorrelationIdByLeave">Корреляции родителей дочерних листов.</param>
    /// <param name="refreshProgress">Обработчик прогресса операции.</param>
    public void Fill(
        IReadOnlyDictionary<Guid, TreeLeaveModel> leavesByCorrelationId,
        IReadOnlyDictionary<TreeLeaveModel, Guid> parentCorrelationIdByLeave,
        Action<int, int> refreshProgress)
    {
        ArgumentNullException.ThrowIfNull(leavesByCorrelationId);
        ArgumentNullException.ThrowIfNull(parentCorrelationIdByLeave);
        ArgumentNullException.ThrowIfNull(refreshProgress);

        var links = parentCorrelationIdByLeave
            .OrderBy(x => GetNodeDepth(x.Key.ParentNode))
            .ToList();

        // UI прогресса не допускает нулевой totalCount. Отсутствие FK-связей
        // означает, что этап уже завершён, а не что у него нулевой диапазон.
        if (links.Count == 0)
        {
            refreshProgress(1, 1);
            return;
        }

        refreshProgress(0, links.Count);
        for (var i = 0; i < links.Count; i++)
        {
            var (childLeave, parentCorrelationId) = links[i];
            if (leavesByCorrelationId.TryGetValue(parentCorrelationId, out var parentLeave) == false)
            {
                throw new InvalidOperationException(
                    $"Для листа '{childLeave.Name}' [{childLeave.Uuid}] не найдена "
                    + $"родительская строка импорта '{parentCorrelationId}'.");
            }

            _service.FillFromParent(childLeave, parentLeave);
            _service.ResolveParent(childLeave);
            refreshProgress(i + 1, links.Count);
        }
    }

    /// <summary>
    /// Возвращает глубину узла для детерминированной обработки сверху вниз.
    /// </summary>
    private static int GetNodeDepth(TreeNodeModel node)
    {
        var depth = 0;
        for (var current = node.ParentNode; current != null; current = current.ParentNode)
            depth++;

        return depth;
    }
}
