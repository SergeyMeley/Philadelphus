namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Загруженный внешний модуль без привязки общего контракта к конкретной среде выполнения.
    /// </summary>
    public sealed class AssemblyAdapterModule
    {
        public required AssemblyAdapterLanguage Language { get; init; }

        public required string SourcePath { get; init; }

        public object? LoadedArtifact { get; init; }
    }
}
