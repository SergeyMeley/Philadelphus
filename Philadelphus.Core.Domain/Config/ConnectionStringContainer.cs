using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Config
{
    public class ConnectionStringContainer
    {
        public Guid Uuid { get; set; }
        public string ProviderName { get; set; }
        public string ConnectionString { get; set; }
        
    }
}
