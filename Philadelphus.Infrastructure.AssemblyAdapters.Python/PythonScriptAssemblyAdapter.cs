using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.Python
{
    /// <summary>
    /// Зарезервированный адаптер Python-скриптов. Выполнение в среде Python намеренно не реализовано на первом этапе.
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
                        Message = "Адаптеры среды Python зарезервированы для будущего этапа и не загружаются на первом этапе."
                    }
                ]);
        }
    }
}
