using Philadelphus.Infrastructure.AssemblyAdapters;

namespace Philadelphus.Infrastructure.AssemblyAdapters.JavaScript
{
    /// <summary>
    /// Зарезервированный адаптер JavaScript-скриптов. Выполнение в среде JavaScript намеренно не реализовано на первом этапе.
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
                        Message = "Адаптеры среды JavaScript зарезервированы для будущего этапа и не загружаются на первом этапе."
                    }
                ]);
        }
    }
}
