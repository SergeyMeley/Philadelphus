using AutoMapper.Configuration.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Configurations
{
    /// <summary>
    /// Параметры конфигурации RedisOptions.
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Хост.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Порт.
        /// </summary>
        public int Port { get; set; } = 6379;

        /// <summary>
        /// Сокет.
        /// </summary>
        [JsonIgnore]
        public string Socket => $"{Host}:{Port}";

        /// <summary>
        /// Пароль.
        /// </summary>
        public string Password { get; set; } = "philapass123";
    }
}
