namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Метаданные расширения
    /// </summary>
    public interface IExtensionMetadataModel
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string Version { get; }
        bool AutoStart { get; }
    }
}
