using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Методы расширения для Enum.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Получить DisplayName для элемента Enum.
        /// </summary>
        /// <param name="value">Элемент</param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DisplayAttribute>();
            return attribute?.Name ?? value.ToString();
        }
    }
}
