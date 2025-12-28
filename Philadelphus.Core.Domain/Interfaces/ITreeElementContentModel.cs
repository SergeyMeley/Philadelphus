using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Interfaces
{
    internal interface ITreeElementContentModel
    {
        IAttributeOwnerModel Owner { get; }
    }
}
