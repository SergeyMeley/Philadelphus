using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public class KafkaOptions<TSubsection>
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string Topic { get; set; } = "philadelphus-general";
        internal static string DefaultTopic { get; set; } = "philadelphus-general";
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Latest;
    }
}
