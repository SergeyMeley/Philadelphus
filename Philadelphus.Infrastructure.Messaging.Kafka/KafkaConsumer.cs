using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Infrastructure.Messaging;
using Serilog;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public class KafkaConsumer<TMessage> : IMessageConsumer<TMessage>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConsumer<string,  TMessage> _consumer;
        private readonly string? _topic;
        private bool _disposed;

        public KafkaConsumer(ILogger logger, IOptions<KafkaOptions> options)
        {
            _logger = logger;

            var config = new ConsumerConfig()
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = $"consumer-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<string, TMessage>(config)
                .SetValueDeserializer(new KafkaJsonDeserializer<TMessage>())
                .Build();

            _topic = options.Value.Topic ?? KafkaOptions.DefaultTopic;
        }

        public async Task StartAsync(Func<TMessage, CancellationToken, Task> handler, CancellationToken ct = default)
        {
            _consumer.Subscribe(_topic);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        _consumer.Subscribe(_topic);
                        break;
                    }
                    catch(Exception)
                    {
                        _logger.Warning($"Топик {_topic} не готов, повторная попытка через 1 сек.");
                        await Task.Delay(1000, ct);
                    }
                }

                while (ct.IsCancellationRequested == false)
                {
                    var resultTask = Task.Run(() => 
                    {
                        return _consumer.Consume(TimeSpan.FromSeconds(1));
                    });

                    if (await Task.WhenAny(resultTask, Task.Delay(1000, ct)) == resultTask)
                    {
                        var result = await resultTask;
                        if (result != null)
                        {
                            var message = result.Message.Value;
                            await handler(message, ct);
                            _consumer.Commit(result);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение по CancellationToken
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка цикла чтения сообщений.");
                throw;
            }
        }
        public void Dispose()
        {
            if (_disposed) 
                return;
            _consumer?.Close();
            _consumer?.Dispose();
            _disposed = true;
        }
    }
}
