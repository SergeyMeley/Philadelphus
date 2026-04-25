using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Infrastructure.Messaging;
using Philadelphus.Infrastructure.Messaging.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public class KafkaProducer<TMessage> : IMessageProducer<TMessage>, IDisposable
    {
        private readonly IProducer<string, TMessage> _producer;
        private readonly string _topic;
        private bool _disposed;

        public KafkaProducer(IOptions<KafkaOptions<TMessage>> options)
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = options.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<string, TMessage>(config)
                .SetValueSerializer(new KafkaJsonSerializer<TMessage>())
                .Build();

            _topic = options.Value.Topic ?? KafkaOptions<TMessage>.DefaultTopic;
        }
        public Task ProduceAsync(TMessage message, CancellationToken cancellationToken, string? key = null)
        {
            var mes = new Message<string, TMessage>()
            {
                Key = key ?? Guid.CreateVersion7().ToString(),
                Value = message
            };
            return _producer.ProduceAsync(_topic, mes, cancellationToken);
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            _producer.Dispose();
            _disposed = true;
        }
    }
}
