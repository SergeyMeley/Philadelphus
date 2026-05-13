using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Infrastructure.Messaging;
using Serilog;

namespace Philadelphus.Infrastructure.Messaging.Kafka
{
    public class KafkaConsumer<TMessage> : BackgroundService, IMessageConsumer<TMessage>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConsumer<string,  TMessage> _consumer;
        private readonly string? _topic;
        private bool _disposed;
        public event Func<TMessage, CancellationToken, Task>? MessageReceived;

        public KafkaConsumer(ILogger logger, IOptions<KafkaOptions<TMessage>> options)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);

            _logger = logger;

            var config = new ConsumerConfig()
            {
                BootstrapServers = options.Value.BootstrapServers,
                GroupId = $"consumer-{Guid.CreateVersion7()}",
                AutoOffsetReset = options.Value.AutoOffsetReset
            };

            _consumer = new ConsumerBuilder<string, TMessage>(config)
                .SetValueDeserializer(new KafkaJsonDeserializer<TMessage>())
                .Build();

            _topic = options.Value.Topic ?? KafkaOptions<TMessage>.DefaultTopic;
        }

        public void Dispose()
        {
            if (_disposed) 
                return;
            _consumer?.Close();
            _consumer?.Dispose();
            _disposed = true;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _consumer.Subscribe(_topic);

            try
            {

                while (ct.IsCancellationRequested == false)
                {
                    ConsumeResult<string, TMessage> result = null;
                    try
                    {
                        result = _consumer.Consume(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.Information("Отмена получения сообщений Kafka.");
                        break;
                    }

                    if (result?.Message != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                if (MessageReceived != null)
                                {
                                    await MessageReceived.Invoke(result.Message.Value, CancellationToken.None);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Ошибка обработчика события.");
                            }
                        });
                        _consumer.Commit(result);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, "Неизвестная ошибка Kafka.");
                throw;
            }
            finally
            {
                _consumer.Close();
                _logger.Information("Остановка получения сообщения.");
            }
        }
    }
}
