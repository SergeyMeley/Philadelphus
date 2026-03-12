using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging.Messages
{
    public class MessageWrapper<TMessage>
    {
        public TMessage Message { get; set; }
        public Guid ConsumerUuid { get; set; }
    }
}
