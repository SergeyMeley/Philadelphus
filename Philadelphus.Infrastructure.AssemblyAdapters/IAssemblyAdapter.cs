namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Загружает внешние модули кода для одного семейства среды выполнения.
    /// </summary>
    public interface IAssemblyAdapter
    {
        AssemblyAdapterLanguage Language { get; }

        IReadOnlyCollection<string> SupportedFileExtensions { get; }

        AssemblyAdapterLoadResult Load(AssemblyAdapterLoadRequest request);
    }
}
