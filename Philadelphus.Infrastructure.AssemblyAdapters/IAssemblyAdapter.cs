namespace Philadelphus.Infrastructure.AssemblyAdapters
{
    /// <summary>
    /// Loads external code modules for one runtime family.
    /// </summary>
    public interface IAssemblyAdapter
    {
        AssemblyAdapterLanguage Language { get; }

        IReadOnlyCollection<string> SupportedFileExtensions { get; }

        AssemblyAdapterLoadResult Load(AssemblyAdapterLoadRequest request);
    }
}
