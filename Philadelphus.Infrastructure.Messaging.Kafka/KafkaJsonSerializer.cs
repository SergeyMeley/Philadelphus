using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Представляет объект KafkaJsonSerializer.
    /// </summary>
    internal class KafkaJsonSerializer<TMessage> : ISerializer<TMessage>
    {
        /// <summary>
        /// Сериализовать сообщение.
        /// </summary>
        /// <param name="data">Данные.</param>
        /// <param name="context">Контекст операции.</param>
        /// <returns>Коллекция полученных данных.</returns>
        public byte[] Serialize(
            TMessage data,
            SerializationContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data);
        }
    }
}
