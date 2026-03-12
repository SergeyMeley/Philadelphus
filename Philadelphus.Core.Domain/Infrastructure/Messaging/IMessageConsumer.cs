using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging
{
    public interface IMessageConsumer<TMessage>
    {
        public Task StartAsync(Func<TMessage, CancellationToken, Task> handler, CancellationToken ct = default);
    }
}
