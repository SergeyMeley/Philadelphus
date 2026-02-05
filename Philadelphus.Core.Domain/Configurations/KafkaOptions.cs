using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Configurations
{
    public class KafkaOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 9092;

        [JsonIgnore]
        public string Socket => $"{Host}:{Port}";
    }
}
