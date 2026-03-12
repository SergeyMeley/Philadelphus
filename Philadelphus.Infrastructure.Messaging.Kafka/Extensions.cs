using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Infrastructure.Messaging;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public static class Extensions
    {
        public static void AddKafkaProducer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
        { 
            services.Configure<KafkaOptions>(configurationSection);
            services.AddSingleton<IMessageProducer<TMessage>, KafkaProducer<TMessage>>();
        }

        public static void AddKafkaConsumer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<KafkaOptions>(configurationSection);
            services.AddSingleton<IMessageConsumer<TMessage>, KafkaConsumer<TMessage>>();
        }
    }
}
