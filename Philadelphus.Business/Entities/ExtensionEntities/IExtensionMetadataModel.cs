using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.ExtensionEntities
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
