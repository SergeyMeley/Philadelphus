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
        IReadOnlySet<Guid> valueUuids,
        bool matchesAnyValue = false)
    {
        DeclaringUuid = declaringUuid;
        IsCollection = isCollection;
        ValueUuid = valueUuid;
        ValueUuids = valueUuids;
        MatchesAnyValue = matchesAnyValue;
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
    /// Указывает, что значение объявления не участвует в сравнении.
    /// </summary>
    public bool MatchesAnyValue { get; }

    /// <summary>
    /// Указывает, что черновик содержит точное пустое значение.
    /// </summary>
    public bool IsEmpty => MatchesAnyValue == false
        && (IsCollection ? ValueUuids.Count == 0 : ValueUuid is null);

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

    /// <summary>
    /// Создаёт критерий, принимающий любое значение указанного объявления.
    /// </summary>
    public static LeaveAttributeValueDraft Any(Guid declaringUuid, bool isCollection)
    {
        ValidateDeclaringUuid(declaringUuid);
        return new(
            declaringUuid,
            isCollection,
            null,
            Array.Empty<Guid>().ToFrozenSet(),
            matchesAnyValue: true);
    }

    private static void ValidateDeclaringUuid(Guid declaringUuid)
    {
        if (declaringUuid == Guid.Empty)
            throw new ArgumentException("UUID объявления не может быть пустым.", nameof(declaringUuid));
    }
}
