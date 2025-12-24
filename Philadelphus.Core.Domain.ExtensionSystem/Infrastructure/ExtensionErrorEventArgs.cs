using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public class ExtensionErrorEventArgs : EventArgs
    {
        public string ExtensionName { get; set; }
        public Exception Exception { get; set; }
    }
}
