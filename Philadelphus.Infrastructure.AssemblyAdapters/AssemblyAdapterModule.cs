namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// A loaded external module, represented without binding the common contract to one runtime.
    /// </summary>
    public sealed class AssemblyAdapterModule
    {
        public required AssemblyAdapterLanguage Language { get; init; }

        public required string SourcePath { get; init; }

        public object? LoadedArtifact { get; init; }
    }
}
