namespace Philadelphus.Core.Domain.Policies.Rules
{
    internal readonly record struct SequencedItem(Guid Uuid, long Sequence);
}
