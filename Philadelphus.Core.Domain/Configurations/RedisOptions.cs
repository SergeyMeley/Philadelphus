using AutoMapper.Configuration.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Configurations
{
    public class RedisOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6379;

        [JsonIgnore]
        public string Socket => $"{Host}:{Port}";
        public string Password { get; set; } = "philapass123";
    }
}
