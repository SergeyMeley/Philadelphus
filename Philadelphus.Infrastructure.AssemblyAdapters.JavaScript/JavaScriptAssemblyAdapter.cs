using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.JavaScript
{
    /// <summary>
    /// Reserved JavaScript script adapter. Runtime execution is intentionally not implemented in stage 1.
    /// </summary>
    public sealed class JavaScriptAssemblyAdapter : IAssemblyAdapter
    {
        public AssemblyAdapterLanguage Language => AssemblyAdapterLanguage.JavaScript;

        public IReadOnlyCollection<string> SupportedFileExtensions { get; } = [".js", ".mjs", ".cjs"];

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
                        Message = "JavaScript runtime adapters are reserved for a future stage and are not loaded in stage 1."
                    }
                ]);
        }
    }
}
