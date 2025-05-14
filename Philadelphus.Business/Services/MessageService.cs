using Philadelphus.Business.Entities.OtherEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services
{
    public static class MessageService
    {
        public static ObservableCollection<Message> Messages { get; set; } = new ObservableCollection<Message>();
    }
}
