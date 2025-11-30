using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Запись в журнал операций
    /// </summary>
    public class OperationLog
    {
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public string Details { get; set; }
        public bool IsError { get; set; }
    }
}
