using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;

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
    /// Создаёт отсутствующие листья полиморфных родителей по всей цепочке узлов.
    /// </summary>
    /// <param name="childLeave">Лист, значения которого используются для создания родителей.</param>
    /// <returns>Созданные листья в порядке от непосредственного родителя к верхнему.</returns>
    /// <exception cref="InvalidOperationException">
    /// Если хотя бы на одном уровне связь невалидна или неоднозначна.
    /// </exception>
    IReadOnlyList<TreeLeaveModel> CreateParentChain(TreeLeaveModel childLeave);

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
    /// Последовательно перестраивает runtime-связи переданных листов.
    /// </summary>
    /// <param name="leaves">Листы для восстановления связей.</param>
    /// <returns>Результат разрешения каждого переданного листа.</returns>
    IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(IEnumerable<TreeLeaveModel> leaves);
}
