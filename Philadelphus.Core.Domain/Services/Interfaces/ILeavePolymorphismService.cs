using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Core.Domain.Services.Interfaces;

/// <summary>
/// Вычисляет и восстанавливает runtime-связи полиморфных листов.
/// </summary>
public interface ILeavePolymorphismService
{
    /// <summary>
    /// Указывает, что сервис в данный момент применяет каскадное обновление.
    /// </summary>
    /// <remarks>
    /// Обработчики изменений атрибутов используют признак, чтобы не запускать
    /// повторный каскад для уведомлений, возникших внутри текущей операции.
    /// </remarks>
    bool IsPropagationInProgress { get; }

    /// <summary>
    /// Вычисляет родителя одного листа и обновляет его runtime-связь.
    /// </summary>
    /// <param name="childLeave">Лист, для которого выполняется поиск.</param>
    /// <returns>Статус разрешения и найденные кандидаты.</returns>
    LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave);

    /// <summary>
    /// Вычисляет родительский лист узла по значениям его унаследованных атрибутов.
    /// </summary>
    /// <param name="childNode">Узел, для которого выполняется поиск.</param>
    /// <returns>Статус разрешения runtime-связи.</returns>
    LeavePolymorphismStatus ResolveParent(TreeNodeModel childNode);

    /// <summary>
    /// Без изменения элемента рассчитывает число унаследованных атрибутов,
    /// которые будут заполнены выбранным полиморфным родителем.
    /// </summary>
    /// <param name="childOwner">Заполняемый узел или лист-наследник.</param>
    /// <param name="parentLeave">Выбранный лист прямого родительского узла.</param>
    /// <returns>Количество изменяемых атрибутов.</returns>
    int CountFillFromParentChanges(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave);

    /// <summary>
    /// Перезаписывает атрибуты прямого родительского уровня значениями выбранного листа.
    /// Runtime-связь после операции должна быть отдельно пересчитана через
    /// <see cref="ResolveParent" />.
    /// </summary>
    /// <param name="childOwner">Заполняемый узел или лист-наследник.</param>
    /// <param name="parentLeave">Выбранный лист прямого родительского узла.</param>
    /// <returns>Сведения о фактически изменённых атрибутах.</returns>
    LeaveAttributeFillResult FillFromParent(
        IAttributeOwnerModel childOwner,
        TreeLeaveModel parentLeave);

    /// <summary>
    /// Создаёт отсутствующие листья полиморфных родителей по всей цепочке узлов.
    /// </summary>
    /// <param name="childLeave">Лист, значения которого используются для создания родителей.</param>
    /// <returns>Созданные листья в порядке от непосредственного родителя к верхнему.</returns>
    /// <exception cref="InvalidOperationException">
    /// Если хотя бы на одном уровне связь невалидна или неоднозначна.
    /// </exception>
    IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeLeaveModel childLeave);

    /// <summary>
    /// Создаёт отсутствующий родительский лист узла и недостающую цепочку выше.
    /// </summary>
    /// <param name="childNode">Узел, значения атрибутов которого переносятся в новый лист.</param>
    /// <returns>Созданные листья от непосредственного родителя к верхнему уровню.</returns>
    IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeNodeModel childNode);

    /// <summary>
    /// Без мутаций рассчитывает транзитивное обновление текущих разрешённых наследников.
    /// </summary>
    /// <param name="changedParentLeave">Изменённый родительский лист.</param>
    /// <returns>Неизменяемый план обновления сверху вниз.</returns>
    LeavePolymorphismPropagationPlan BuildPropagationPlan(
        TreeLeaveModel changedParentLeave);

    /// <summary>
    /// Применяет подтверждённый план обновления сверху вниз и перестраивает runtime-связи.
    /// </summary>
    /// <param name="plan">Предварительно рассчитанный неизменяемый план.</param>
    /// <exception cref="InvalidOperationException">
    /// Если операция вызвана повторно до завершения предыдущего применения.
    /// </exception>
    void ApplyPropagation(LeavePolymorphismPropagationPlan plan);

    /// <summary>
    /// Сохраняет прежние значения наследников и разрешает для них заменяющих родителей.
    /// </summary>
    /// <param name="plan">Отклонённый пользователем план каскадного обновления.</param>
    /// <returns>Итоговые статусы непосредственных наследников и созданные листы.</returns>
    LeavePolymorphismPreservationResult PreserveChildrenAndResolveReplacement(
        LeavePolymorphismPropagationPlan plan);

    /// <summary>
    /// Последовательно перестраивает runtime-связи переданных листов.
    /// </summary>
    /// <param name="leaves">Листы для восстановления связей.</param>
    /// <returns>Результат разрешения каждого переданного листа.</returns>
    IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(IEnumerable<TreeLeaveModel> leaves);

    /// <summary>
    /// Последовательно перестраивает runtime-связи переданных узлов.
    /// </summary>
    /// <param name="nodes">Узлы для восстановления связей.</param>
    void RefreshLinks(IEnumerable<TreeNodeModel> nodes);
}
