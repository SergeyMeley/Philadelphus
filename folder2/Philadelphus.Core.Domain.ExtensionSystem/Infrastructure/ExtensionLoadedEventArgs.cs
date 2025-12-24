using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    public class ExtensionLoadedEventArgs : EventArgs
    {
        public ExtensionInstance Extension { get; set; }
    }
}
