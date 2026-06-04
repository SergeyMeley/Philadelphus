namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Некритичная ошибка загрузки, возвращенная адаптером.
    /// </summary>
    public sealed class AssemblyAdapterLoadError
    {
        public required AssemblyAdapterLanguage Language { get; init; }

        public required string SourcePath { get; init; }

        public required string Message { get; init; }

        public Exception? Exception { get; init; }
    }
}
