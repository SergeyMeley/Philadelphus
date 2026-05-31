namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Describes a module load request for an assembly/script adapter.
    /// </summary>
    public sealed class AssemblyAdapterLoadRequest
    {
        public required string Path { get; init; }

        public bool Recursive { get; init; }
    }
}
