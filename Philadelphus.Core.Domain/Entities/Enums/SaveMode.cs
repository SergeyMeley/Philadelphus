using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Перечисляет варианты SaveMode.
    /// </summary>
    public enum SaveMode
    {
        OnlyHeader,
        WithContent,
        WithContentAndMembers
    }
}
