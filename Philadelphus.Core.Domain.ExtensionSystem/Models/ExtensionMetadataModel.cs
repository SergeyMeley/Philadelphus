using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Models
{
    /// <summary>
    /// Метаданные расширения (реализация)
    /// </summary>
    public class ExtensionMetadataModel : IExtensionMetadataModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool AutoStart { get; set; }

        public ExtensionMetadataModel(string id, string name, string description, string version, bool autoStart = false)
        {
            Id = id;
            Name = name;
            Description = description;
            Version = version;
            AutoStart = autoStart;
        }
    }
}
