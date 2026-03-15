using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging
{
    public interface IMessageConsumer<TMessage>
    {
        public event Func<TMessage, CancellationToken, Task>? MessageReceived;
    }
}
