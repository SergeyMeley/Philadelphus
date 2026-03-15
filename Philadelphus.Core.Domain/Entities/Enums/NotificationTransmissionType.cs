using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.Enums
{
    /// <summary>
    /// Тип передачи данных
    /// </summary>
    public enum NotificationTransmissionType
    {
        /// <summary>
        /// Передача текущему клиенту
        /// </summary>
        [Display(Name = "Себе")]
        Self,
        ///// <summary>
        ///// Передача одному конкретному получателю
        ///// </summary>
        //[Display(Name = "Одному получателю")]
        //Unicast,
        ///// <summary>
        ///// Передача ограниченной группе получателей
        ///// </summary>
        //[Display(Name = "Группе получателей")]
        //Multicast,
        /// <summary>
        /// Широковещательная передача всем получателям
        /// </summary>
        [Display(Name = "Всем")]
        Broadcast
    }
}
