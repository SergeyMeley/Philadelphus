namespace Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

/// <summary>
/// Результат вычисления полиморфного родителя листа.
/// </summary>
public enum LeavePolymorphismStatus
{
    Invalid,
    NotFound,
    Ambiguous,
    Resolved
}
