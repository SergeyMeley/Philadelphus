using System.Collections.Frozen;

namespace Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;

/// <summary>
/// Неизменяемое значение атрибута для поиска или создания листа.
/// </summary>
public sealed record LeaveAttributeValueDraft
{
    private LeaveAttributeValueDraft(
        Guid declaringUuid,
        bool isCollection,
        Guid? valueUuid,
        IReadOnlySet<Guid> valueUuids)
    {
        DeclaringUuid = declaringUuid;
        IsCollection = isCollection;
        ValueUuid = valueUuid;
        ValueUuids = valueUuids;
    }

    /// <summary>
    /// Идентификатор объявления атрибута.
    /// </summary>
    public Guid DeclaringUuid { get; }

    /// <summary>
    /// Признак коллекционного значения.
    /// </summary>
    public bool IsCollection { get; }

    /// <summary>
    /// UUID скалярного значения или null для пустого значения.
    /// </summary>
    public Guid? ValueUuid { get; }

    /// <summary>
    /// Множество UUID коллекционного значения.
    /// </summary>
    public IReadOnlySet<Guid> ValueUuids { get; }

    /// <summary>
    /// Создаёт черновик скалярного значения.
    /// </summary>
    /// <param name="declaringUuid">UUID объявления атрибута.</param>
    /// <param name="valueUuid">UUID значения или null для пустого значения.</param>
    /// <returns>Скалярный черновик.</returns>
    public static LeaveAttributeValueDraft Scalar(
        Guid declaringUuid,
        Guid? valueUuid)
    {
        ValidateDeclaringUuid(declaringUuid);
        if (valueUuid == Guid.Empty)
            throw new ArgumentException("UUID значения не может быть пустым.", nameof(valueUuid));

        return new(declaringUuid, false, valueUuid, Array.Empty<Guid>().ToFrozenSet());
    }

    /// <summary>
    /// Создаёт черновик коллекционного значения без учёта порядка и повторов.
    /// </summary>
    /// <param name="declaringUuid">UUID объявления атрибута.</param>
    /// <param name="valueUuids">UUID значений коллекции.</param>
    /// <returns>Коллекционный черновик.</returns>
    public static LeaveAttributeValueDraft Collection(
        Guid declaringUuid,
        IEnumerable<Guid> valueUuids)
    {
        ValidateDeclaringUuid(declaringUuid);
        ArgumentNullException.ThrowIfNull(valueUuids);

        var values = valueUuids.ToFrozenSet();
        if (values.Contains(Guid.Empty))
            throw new ArgumentException("UUID значения не может быть пустым.", nameof(valueUuids));

        return new(declaringUuid, true, null, values);
    }

    private static void ValidateDeclaringUuid(Guid declaringUuid)
    {
        if (declaringUuid == Guid.Empty)
            throw new ArgumentException("UUID объявления не может быть пустым.", nameof(declaringUuid));
    }
}
