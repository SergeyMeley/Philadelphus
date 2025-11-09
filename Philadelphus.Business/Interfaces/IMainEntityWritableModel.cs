using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Interfaces
{
    internal interface IMainEntityWritableModel : IMainEntityModel
    {
        internal bool SetState(State newState);
    }
}
