using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Entities.LeavePolymorphism;

/// <summary>
/// Runtime-атрибут выбора листа прямого родительского узла.
/// </summary>
public sealed class LeavePolymorphismAttributeModel : ElementAttributeModel
{
    private IReadOnlyList<TreeLeaveModel> _candidates = Array.Empty<TreeLeaveModel>();

    /// <summary>
    /// Создаёт объявление или материализованную копию служебного атрибута.
    /// </summary>
    internal LeavePolymorphismAttributeModel(
        Guid localUuid,
        IAttributeOwnerModel localOwner,
        Guid declaringUuid,
        TreeNodeModel declaringOwner,
        WorkingTreeModel owningWorkingTree,
        INotificationService notificationService,
        IPropertiesPolicy<ElementAttributeModel> propertiesPolicy)
        : base(
            localUuid,
            localOwner,
            declaringUuid,
            declaringOwner,
            owningWorkingTree,
            notificationService,
            propertiesPolicy)
    {
    }

    /// <summary>
    /// Признак атрибута, существующего только во время работы приложения.
    /// </summary>
    public bool IsRuntime => true;

    /// <summary>
    /// Признак служебного атрибута полиморфной связи.
    /// </summary>
    public bool IsPolymorphic => true;

    /// <summary>
    /// Имя всегда совпадает с актуальным именем прямого родительского узла.
    /// </summary>
    public override string Name
    {
        get => ParentNode.Name;
        set { }
    }

    /// <summary>
    /// Тип данных всегда является прямым родительским узлом.
    /// </summary>
    public override TreeNodeModel ValueType
    {
        get => ParentNode;
        set { }
    }

    /// <summary>
    /// Полиморфный родитель всегда выбирается как одно значение.
    /// </summary>
    public override bool IsCollectionValue
    {
        get => false;
        set { }
    }

    /// <summary>
    /// Значение вычисляется из runtime-связи владельца атрибута.
    /// </summary>
    public override TreeLeaveModel Value
    {
        get => Owner switch
        {
            TreeNodeModel node => node.PolymorphicParentLeave!,
            TreeLeaveModel leave => leave.PolymorphicParentLeave!,
            _ => null!
        };
        set { }
    }

    /// <summary>
    /// Атрибут наследуется потомками как единая служебная строка.
    /// </summary>
    public override VisibilityScope Visibility
    {
        get => VisibilityScope.Protected;
        set { }
    }

    /// <summary>
    /// Текущий статус вычисления полиморфного родителя.
    /// </summary>
    public LeavePolymorphismStatus Status { get; private set; } =
        LeavePolymorphismStatus.NotFound;

    /// <summary>
    /// Найденные по значениям атрибутов кандидаты.
    /// </summary>
    public IReadOnlyList<TreeLeaveModel> Candidates => _candidates;

    /// <inheritdoc />
    public override ElementAttributeModel CloneForChild(IAttributeOwnerModel newOwner)
    {
        ArgumentNullException.ThrowIfNull(newOwner);

        return new LeavePolymorphismAttributeModel(
            Guid.CreateVersion7(),
            newOwner,
            DeclaringUuid,
            (TreeNodeModel)DeclaringOwner,
            OwningWorkingTree,
            _notificationService,
            _propertiesPolicy);
    }

    /// <summary>
    /// Обновляет runtime-диагностику после поиска родителя.
    /// </summary>
    internal void SetResolution(
        LeavePolymorphismStatus status,
        IReadOnlyList<TreeLeaveModel> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        Status = status;
        _candidates = candidates.ToList().AsReadOnly();
    }

    /// <summary>
    /// Прямой родитель локального узла, которому принадлежит материализованный атрибут.
    /// </summary>
    private TreeNodeModel ParentNode =>
        Owner switch
        {
            TreeNodeModel node => node.ParentNode,
            TreeLeaveModel leave => leave.ParentNode.ParentNode,
            _ => null!
        };
}
