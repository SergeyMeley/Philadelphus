using Confluent.Kafka;
using System.Text.Json;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Представляет объект KafkaJsonDeserializer.
    /// </summary>
    internal class KafkaJsonDeserializer<TMessage> : IDeserializer<TMessage>
    {
        /// <summary>
        /// Десериализовать сообщение.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <param name="isNull">Признак null-значения.</param>
        /// <param name="context">Контекст операции.</param>
        /// <returns>Результат выполнения операции.</returns>
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