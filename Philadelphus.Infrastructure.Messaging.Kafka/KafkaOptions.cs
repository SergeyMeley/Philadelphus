using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    /// <summary>
    /// Параметры конфигурации KafkaOptions.
    /// </summary>
    public class KafkaOptions<TSubsection>
    {
        /// <summary>
        /// Адреса bootstrap-серверов Kafka.
        /// </summary>
        public string BootstrapServers { get; set; } = "localhost:9092";
        
        /// <summary>
        /// Топик сообщений.
        /// </summary>
        public string Topic { get; set; } = "philadelphus-general";
        
        /// <summary>
        /// Топик по умолчанию.
        /// </summary>
        internal static string DefaultTopic { get; set; } = "philadelphus-general";
        
        /// <summary>
        /// Правило сброса смещения Kafka.
        /// </summary>
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Latest;
    }
}
