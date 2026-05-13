using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Infrastructure.Messaging
{
    /// <summary>
    /// Задает контракт для отправителя сообщения.
    /// </summary>
    public interface IMessageProducer<TMessage>
    {
        /// <summary>
        /// Выполняет операцию ProduceAsync.
        /// </summary>
        /// <param name="message">Сообщение.</param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <param name="key">Ключ.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public Task ProduceAsync(TMessage message, CancellationToken cancellationToken, string? key = null);
    }
}
