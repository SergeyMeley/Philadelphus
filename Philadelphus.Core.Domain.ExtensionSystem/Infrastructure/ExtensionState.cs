using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.ExtensionSystem.Infrastructure
{
    /// <summary>
    /// Состояния расширения
    /// </summary>
    public enum ExtensionState
    {
        Created,    // Создано, но не запущено
        Running,    // Работает
        Stopped,    // Остановлено
        Error       // Ошибка
    }
}
