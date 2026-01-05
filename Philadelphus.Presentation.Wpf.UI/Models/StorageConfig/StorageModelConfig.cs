using Philadelphus.Infrastructure.Persistence.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Models.StorageConfig
{
    public class StorageModelConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        //public string UuidString { get => Uuid.ToString(); set => Uuid = Guid.Parse(UuidString); }
        public Guid Uuid { get; set; }//{ get => Guid.Parse(UuidString); set => UuidString = value.ToString(); }
        //public string ConnectionString { get; set; } = string.Empty;
        //public string TypeName { get; set; } = string.Empty;
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
        public bool IsHidden { get; set; }
        //public string? DatabaseName { get; set; }
        //public bool IsEnabled { get; set; } = true;
        //public int Priority { get; set; } = 1;
        //public Dictionary<string, string>? AdditionalParameters { get; set; }
    }
}
