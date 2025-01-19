using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Enums
{
    public enum EntityTypes
    {
        None = -1,
        Repository,
        Root,
        Node,
        Leave,
        Attribute,
        AttributeEntry,
        AttributeValue,
    }
}
