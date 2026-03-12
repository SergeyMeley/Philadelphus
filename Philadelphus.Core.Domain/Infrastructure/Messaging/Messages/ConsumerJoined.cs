using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging.Messages
{
    public class ConsumerJoined
    {
        public required Guid ProducerUuid { get; init; }
        public required string ProducerName { get; init; }
        public required DateTime JoinDateTime { get; init; }
    }
}
