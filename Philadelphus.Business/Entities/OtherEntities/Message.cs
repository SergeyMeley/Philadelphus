using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.OtherEntities
{
    public class Message
    {
        public MessageTypes MessageType { get; init; }
        public string Text { get; init; }
        public DateTime DateTime { get; init; }
        public Message(MessageTypes type, string text)
        {
            MessageType = type;
            Text = text;
            DateTime = DateTime.Now;
        }
    }
}
