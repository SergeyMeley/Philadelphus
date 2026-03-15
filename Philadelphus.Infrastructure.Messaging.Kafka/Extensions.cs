using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Infrastructure.Messaging;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public static class Extensions
    {
        public static void AddKafkaProducer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
        { 
            services.Configure<KafkaOptions<TMessage>>(configurationSection);
            services.AddSingleton<IMessageProducer<TMessage>, KafkaProducer<TMessage>>();
        }

        public static void AddKafkaConsumer<TMessage>(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<KafkaOptions<TMessage>>(configurationSection);
            services.AddSingleton<KafkaConsumer<TMessage>>();
            services.AddHostedService(sp => sp.GetRequiredService<KafkaConsumer<TMessage>>());
            services.AddSingleton<IMessageConsumer<TMessage>>(sp => sp.GetRequiredService<KafkaConsumer<TMessage>>());
        }
    }
}
