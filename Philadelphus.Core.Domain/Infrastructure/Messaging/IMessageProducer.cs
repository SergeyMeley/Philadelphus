using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging
{
    public interface IMessageProducer<TMessage>
    {
        public Task ProduceAsync(TMessage message, CancellationToken cancellationToken, string? key = null);
    }
}
