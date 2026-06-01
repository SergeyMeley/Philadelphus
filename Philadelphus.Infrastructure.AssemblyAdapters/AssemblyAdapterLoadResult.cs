namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Result of loading external modules. Errors do not prevent other modules from loading.
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
