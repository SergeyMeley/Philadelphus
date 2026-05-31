using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.Python
{
    /// <summary>
    /// Reserved Python script adapter. Runtime execution is intentionally not implemented in stage 1.
    /// </summary>
    public sealed class PythonScriptAssemblyAdapter : IAssemblyAdapter
    {
        public AssemblyAdapterLanguage Language => AssemblyAdapterLanguage.Python;

        public IReadOnlyCollection<string> SupportedFileExtensions { get; } = [".py"];

        public AssemblyAdapterLoadResult Load(AssemblyAdapterLoadRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Path);

            return new AssemblyAdapterLoadResult(
                [],
                [
                    new AssemblyAdapterLoadError
                    {
                        Language = Language,
                        SourcePath = Path.GetFullPath(request.Path),
                        Message = "Python runtime adapters are reserved for a future stage and are not loaded in stage 1."
                    }
                ]);
        }
    }
}
