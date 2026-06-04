using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.CSharp
{
    /// <summary>
    /// Результат создания типизированных экземпляров из загруженных C#-сборок.
    /// </summary>
    public sealed class CSharpAssemblyInstanceLoadResult<TContract>
        where TContract : class
    {
        public CSharpAssemblyInstanceLoadResult(
            IReadOnlyList<TContract> instances,
            IReadOnlyList<AssemblyAdapterLoadError> errors)
        {
            Instances = instances;
            Errors = errors;
        }

        public IReadOnlyList<TContract> Instances { get; }

        public IReadOnlyList<AssemblyAdapterLoadError> Errors { get; }

        public bool IsSuccess => Errors.Count == 0;
    }
}
