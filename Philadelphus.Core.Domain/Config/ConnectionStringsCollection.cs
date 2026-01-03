using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Config
{
    public class ConnectionStringsCollection
    {
        public IEnumerable<ConnectionStringContainer> ConnectionStringContainers { get; set; } = new List<ConnectionStringContainer>();
    }
}
