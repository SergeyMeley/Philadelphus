using Confluent.Kafka;
using System.Text.Json;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    internal class KafkaJsonDeserializer<TMessage> : IDeserializer<TMessage>
    {
        public TMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull)
                return default!;

            return JsonSerializer.Deserialize<TMessage>(data,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }
    }
}