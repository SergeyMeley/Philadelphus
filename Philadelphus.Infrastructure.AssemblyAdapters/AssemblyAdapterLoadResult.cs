namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Результат загрузки внешних модулей. Ошибки не прерывают загрузку остальных модулей.
    /// </summary>
    public sealed class AssemblyAdapterLoadResult
    {
        public AssemblyAdapterLoadResult(
            IReadOnlyList<AssemblyAdapterModule> modules,
            IReadOnlyList<AssemblyAdapterLoadError> errors)
        {
            Modules = modules;
            Errors = errors;
        }

        public IReadOnlyList<AssemblyAdapterModule> Modules { get; }

        public IReadOnlyList<AssemblyAdapterLoadError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;
    }
}
