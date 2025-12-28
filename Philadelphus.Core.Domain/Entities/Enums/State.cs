using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    public enum State
    {
        Initialized,
        Changed,
        SavedOrLoaded,
        ForSoftDelete,
        ForHardDelete,
        SoftDeleted,
    }
}
