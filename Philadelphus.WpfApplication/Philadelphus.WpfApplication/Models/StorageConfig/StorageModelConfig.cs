using Philadelphus.InfrastructureEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.Models.StorageConfig
{
    public class StorageModelConfig
    {
        public string TypeName { get; set; } = string.Empty;
        public string GuidString { get; set; } = string.Empty;
        public Guid Guid { get => Guid.Parse(GuidString); set => GuidString = value.ToString(); }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string ProviderTypeString { get; set; } = string.Empty;
        public InfrastructureTypes ProviderType 
        { 
            get
            {
                Enum.TryParse(enumType: typeof(InfrastructureTypes), ProviderTypeString, out object res);
                return (InfrastructureTypes)res;
            }
            set => ProviderTypeString = value.ToString(); 
        }
        public string? DatabaseName { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 1;
        public Dictionary<string, string>? AdditionalParameters { get; set; }
    }
}
